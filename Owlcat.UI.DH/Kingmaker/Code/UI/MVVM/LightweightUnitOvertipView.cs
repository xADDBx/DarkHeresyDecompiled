using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.UI.Sound;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LightweightUnitOvertipView : BaseOvertipView<LightweightUnitOvertipVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipLightweightUnitNameView m_NameBlockPCView;

	[Header("Bark")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[Header("Common Block")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	[SerializeField]
	private float m_FarDistance = 120f;

	[SerializeField]
	private float m_StandardOvertipPositionYCorrection = 30f;

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private IDisposable m_HoverDelay;

	protected override bool CheckVisibility
	{
		get
		{
			if (!base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer.CurrentValue)
			{
				return base.ViewModel.MechanicEntityUIState.IsDead.CurrentValue;
			}
			return true;
		}
	}

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.MechanicEntityUIState.IsCurrentUnitTurn.CurrentValue || base.ViewModel.MechanicEntityUIState.IsActing.CurrentValue) && !base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue && !base.ViewModel.MechanicEntityUIState.IsPingUnit.CurrentValue)
			{
				return base.ViewModel.IsBarkActive.CurrentValue;
			}
			return true;
		}
	}

	private void Awake()
	{
		List<DoNotDisableRaycasts> list = new List<DoNotDisableRaycasts>();
		GetComponentsInChildren(includeInactive: true, list);
		m_MainRaycastBlockers = list.Select((DoNotDisableRaycasts c) => c.GetComponent<Graphic>()).ToList();
		GetComponentsInChildren(includeInactive: true, m_AdditionalRaycastBlockers);
		m_AdditionalRaycastBlockers = m_AdditionalRaycastBlockers.Where((Graphic g) => g.raycastTarget && g.GetComponent<DoNotDisableRaycasts>() == null).ToList();
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
	}

	private void EnableMainRaycasts(bool enable)
	{
		foreach (Graphic mainRaycastBlocker in m_MainRaycastBlockers)
		{
			mainRaycastBlocker.raycastTarget = enable;
		}
	}

	private void EnableAdditionalRaycasts(bool enable)
	{
		foreach (Graphic additionalRaycastBlocker in m_AdditionalRaycastBlockers)
		{
			additionalRaycastBlocker.raycastTarget = enable;
		}
	}

	protected override void OnBind()
	{
		m_InnerCanvasGroup.alpha = 0f;
		base.OnBind();
		base.gameObject.name = base.ViewModel.MechanicEntityUIWrapper.Name + "_UnitOvertip";
		m_NameBlockPCView.Bind(base.ViewModel.NameBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer.CombineLatest(base.ViewModel.IsBarkActive, base.ViewModel.MechanicEntityUIState.IsCurrentUnitTurn, base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.IsAoETarget, base.ViewModel.CameraDistance, (bool _, bool _, bool _, bool _, bool _, bool _, Vector3 _) => true).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			UpdateVisibility();
		})
			.AddTo(this);
		base.ViewModel.SetDeathDelay(0.5f);
		base.ViewModel.MechanicEntityUIState.IsDead.Where((bool v) => v).Subscribe(delegate
		{
			DoDeath();
		}).AddTo(this);
		m_Visibility.Subscribe(DoVisibility).AddTo(this);
		base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.CombineLatest(m_Visibility, (bool tab, UnitOvertipVisibility vis) => new { tab, vis }).Subscribe().AddTo(this);
		EnableMainRaycasts(enable: true);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
		base.OnUnbind();
	}

	private void DoDeath()
	{
		UISounds.Instance.Sounds.Combat.UnitDeath.Play();
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		UnitOvertipVisibilitySettings? unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstOrDefault((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == unitOvertipVisibility);
		float alpha = unitOvertipVisibilitySettings.Value.Alpha;
		float scale = unitOvertipVisibilitySettings.Value.Scale;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_InnerCanvasGroup.DOFade(alpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = m_RectTransform.DOScale(scale, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		PositionCorrectionFromView = new Vector2(0f, 0f - m_StandardOvertipPositionYCorrection);
	}

	private void UpdateVisibility()
	{
		if (base.ViewModel.MechanicEntityUIState.IsDead.CurrentValue)
		{
			return;
		}
		if (!CheckVisibility || base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.CurrentValue.sqrMagnitude < m_FarDistance;
		if (base.ViewModel.MechanicEntityUIState.IsTBM.CurrentValue)
		{
			if (CheckVisibleTrigger)
			{
				m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
			}
			else if (base.ViewModel.MechanicEntityUIState.IsAoETarget.CurrentValue)
			{
				m_Visibility.Value = UnitOvertipVisibility.NotFull;
			}
			else
			{
				m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
			}
		}
		else if (CheckVisibleTrigger)
		{
			m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
		}
		else
		{
			m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
		}
		if (m_Visibility.CurrentValue != 0 && base.ViewModel.IsBarkActive.CurrentValue)
		{
			base.transform.SetAsLastSibling();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.ViewModel != null && base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(delegate
			{
				m_Visibility.Value = UnitOvertipVisibility.Maximized;
			}, 0.2f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.ViewModel != null && base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(UpdateVisibility, 1f);
		}
	}
}
