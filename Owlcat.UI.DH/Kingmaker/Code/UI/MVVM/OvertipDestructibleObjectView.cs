using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Predictions;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Abilities;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipDestructibleObjectView : BaseOvertipView<OvertipDestructibleObjectVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipUnitHealthBlockView m_HealthBlockView;

	[SerializeField]
	private OvertipUnitNameView m_NameBlockPCView;

	[Header("Combat Statuses")]
	[SerializeField]
	private OvertipHitChanceBlockView m_HitChanceBlockPCView;

	[SerializeField]
	private OvertipDamageBlockView m_DamageBlockPCView;

	[Header("Common Block")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	[SerializeField]
	private float m_FarDistance = 120f;

	[Header("Bark and Combat text")]
	[SerializeField]
	private OvertipCombatTextBlockView m_CombatTextBlockPCView;

	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[Header("Death")]
	[SerializeField]
	private FadeAnimator m_DeathAnimator;

	[SerializeField]
	private float m_DeathDelay = 0.2f;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private IDisposable m_HoverDelay;

	protected override bool CheckVisibility => base.ViewModel.IsVisible();

	private bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue)
			{
				return base.ViewModel.HasActiveCombatMessage.CurrentValue;
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
		base.OnBind();
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_OvertipDestructibleOvertip";
		m_HealthBlockView.Bind(base.ViewModel.HealthBlockVM);
		m_NameBlockPCView.Bind(base.ViewModel.NameBlockVM);
		m_HitChanceBlockPCView.Bind(base.ViewModel.HitChanceBlockVM);
		m_DamageBlockPCView.Bind(base.ViewModel.DamageBlockVM);
		m_CombatTextBlockPCView.Bind(base.ViewModel.CombatTextBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer.CombineLatest(base.ViewModel.MechanicEntityUIState.IsCurrentUnitTurn, base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.IsAoETarget, base.ViewModel.CameraDistance, base.ViewModel.MechanicEntityUIState.AbilityTargetUIData, (bool isVisible, bool current, bool hotkeyHighlight, bool hover, bool isAoE, Vector3 distance, AbilityTargetUIData _) => new { isVisible, current, hotkeyHighlight, hover, isAoE, distance }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			UpdateVisibility();
		})
			.AddTo(this);
		base.ViewModel.HasActiveCombatMessage.CombineLatest(base.ViewModel.MechanicEntityUIState.Ability, (bool hasActiveCombatText, AbilityData ability) => new { hasActiveCombatText, ability }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			UpdateVisibility();
		})
			.AddTo(this);
		base.ViewModel.SetDeathDelay(m_DeathDelay + m_DeathAnimator.AppearTime + m_DeathAnimator.DisappearTime);
		(from v in base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.Skip(1)
			where v
			select v).Subscribe(delegate
		{
			DoDeath();
		}).AddTo(this);
		m_Visibility.Subscribe(DoVisibility).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_HealthBlockView.Unbind();
		m_NameBlockPCView.Unbind();
		m_HitChanceBlockPCView.Unbind();
		m_DamageBlockPCView.Unbind();
		m_CombatTextBlockPCView.Unbind();
		m_BarkBlockPCView.Unbind();
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		TryDestroyViewImplementation();
	}

	private void DoDeath()
	{
		if (base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			if (m_DeathAnimator.CanvasGroup != null)
			{
				m_DeathAnimator.CanvasGroup.alpha = 0f;
			}
			return;
		}
		m_DeathAnimator.AppearAnimation(delegate
		{
			DelayedInvoker.InvokeInTime(delegate
			{
				m_DeathAnimator.DisappearAnimation();
			}, m_DeathDelay);
		});
	}

	private void UpdateVisibility()
	{
		m_Visibility.Value = GetVisibilityState();
		if (m_Visibility.CurrentValue != 0)
		{
			m_CombatTextBlockPCView.UpdateVisualForCommon();
		}
	}

	private UnitOvertipVisibility GetVisibilityState()
	{
		if (!CheckVisibility)
		{
			return UnitOvertipVisibility.Invisible;
		}
		if (CheckVisibleTrigger)
		{
			return GetVisibilityStateByDistance();
		}
		if (base.ViewModel.MechanicEntityUIState.IsTarget.CurrentValue)
		{
			if (!base.ViewModel.IsPrimaryTarget())
			{
				return UnitOvertipVisibility.NotFull;
			}
			return UnitOvertipVisibility.Full;
		}
		if (!base.ViewModel.MechanicEntityUIState.IsCover.CurrentValue)
		{
			return GetVisibilityStateByDistance();
		}
		return UnitOvertipVisibility.Invisible;
	}

	private UnitOvertipVisibility GetVisibilityStateByDistance()
	{
		if (!(base.ViewModel.CameraDistance.CurrentValue.sqrMagnitude < m_FarDistance))
		{
			return UnitOvertipVisibility.Near;
		}
		return UnitOvertipVisibility.Full;
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
	}

	private void TryDestroyViewImplementation()
	{
		if (base.ViewModel != null && base.ViewModel.HasActiveCombatMessage.CurrentValue)
		{
			DelayedInvoker.InvokeInFrames(TryDestroyViewImplementation, 5);
			return;
		}
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
		base.OnUnbind();
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
