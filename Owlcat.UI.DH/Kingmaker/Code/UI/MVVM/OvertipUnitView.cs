using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipUnitView : BaseOvertipView<OvertipUnitVM>
{
	[SerializeField]
	private OvertipUnitNameView m_NameBlockPCView;

	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[Space]
	[SerializeField]
	private OvertipUnitHealthBlockView m_HealthBlockView;

	[SerializeField]
	private OvertipHitChanceBlockView m_HitChanceBlockPCView;

	[SerializeField]
	private OvertipDamageBlockView m_DamageBlockPCView;

	[SerializeField]
	private OvertipCoverBlockView m_OvertipCoverBlockView;

	[SerializeField]
	private OvertipCombatTextBlockView m_CombatTextBlockPCView;

	[SerializeField]
	private OvertipSpecialBuffBlockView m_OvertipSpecialBuffBlockView;

	[SerializeField]
	private OvertipConcentrationActionView m_OvertipConcentrationActionView;

	[SerializeField]
	private OvertipMoraleView m_OvertipMoraleView;

	[FormerlySerializedAs("m_AdditionalCombatObjectBlockView")]
	[SerializeField]
	private OvertipCombatUnitInteractionView CombatUnitInteractionView;

	[SerializeField]
	private EntityOvertipVisibilityView m_VisibilityView;

	[SerializeField]
	private OvertipBuffsPredictionBlockView m_OvertipBuffsPredictionBlockView;

	[Space]
	[SerializeField]
	private FadeAnimator m_DeathAnimator;

	[SerializeField]
	private float m_DeathDelay = 0.2f;

	[Space]
	[SerializeField]
	private RectTransform m_RectTransform;

	[Space]
	[SerializeField]
	private float m_HoveredScale = 1.25f;

	[SerializeField]
	private float m_RectTransformHasArmorHeight = 50f;

	[SerializeField]
	private float m_RectTransformNoArmorHeight = 50f;

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private Tween m_ScaleUpTween;

	private Tween m_ScaleDownTween;

	protected override bool CheckVisibility => base.ViewModel.VisibilityVM.IsOvertipActive();

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.name = base.ViewModel.MechanicEntityUIWrapper.Name + "_UnitOvertip";
		m_VisibilityView.Initialize(() => base.ViewportPosition);
		m_NameBlockPCView.Bind(base.ViewModel.NameBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_VisibilityView.Bind(base.ViewModel.VisibilityVM);
		m_CombatTextBlockPCView.Bind(base.ViewModel.CombatTextBlockVM);
		m_HealthBlockView.HideInstant();
		m_OvertipMoraleView.HideInstant();
		m_OvertipSpecialBuffBlockView.HideInstant();
		m_OvertipBuffsPredictionBlockView.HideInstant();
		m_OvertipConcentrationActionView.HideInstant();
		m_DamageBlockPCView.HideInstant();
		m_OvertipCoverBlockView.HideInstant();
		m_HitChanceBlockPCView.Or(null)?.HideInstant();
		CombatUnitInteractionView.HideInstant();
		base.ViewModel.CombatBlocksCreated.Subscribe(delegate(bool created)
		{
			if (created)
			{
				BindCombatViews();
			}
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts).AddTo(this);
		base.ViewModel.SetDeathDelay(m_DeathDelay + m_DeathAnimator.AppearTime + m_DeathAnimator.DisappearTime);
		(from v in base.ViewModel.MechanicEntityUIState.IsDead.Skip(1)
			where v
			select v).Subscribe(delegate
		{
			DoDeath();
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.Subscribe(HandleHovered).AddTo(this);
		base.ViewModel.HasActiveCombatMessage.CombineLatest(base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.IsAoETarget, base.ViewModel.MechanicEntityUIState.IsInCombat, base.ViewModel.IsBarkActive, (bool hasText, bool hover, bool aoe, bool isInCombat, bool isBarkActive) => hasText || isBarkActive || (hover && isInCombat) || aoe).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(bool value)
		{
			if (value)
			{
				base.transform.SetAsLastSibling();
			}
		})
			.AddTo(this);
		EnableMainRaycasts(enable: true);
		UpdateOvertipHeight();
	}

	protected override void OnUnbind()
	{
		m_NameBlockPCView.Unbind();
		m_BarkBlockPCView.Unbind();
		m_VisibilityView.Unbind();
		m_CombatTextBlockPCView.Unbind();
		m_HealthBlockView.Unbind();
		m_OvertipMoraleView.Unbind();
		m_OvertipSpecialBuffBlockView.Unbind();
		m_OvertipBuffsPredictionBlockView.Unbind();
		m_OvertipConcentrationActionView.Unbind();
		m_DamageBlockPCView.Unbind();
		m_OvertipCoverBlockView.Unbind();
		m_HitChanceBlockPCView.Or(null)?.Unbind();
		CombatUnitInteractionView.Unbind();
		m_ScaleUpTween?.Kill();
		m_ScaleDownTween?.Kill();
		m_ScaleUpTween = null;
		m_ScaleDownTween = null;
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
		base.OnUnbind();
	}

	private void BindCombatViews()
	{
		m_HealthBlockView.Bind(base.ViewModel.HealthBlockVM);
		m_OvertipMoraleView.Bind(base.ViewModel.OvertipMoraleVM);
		m_OvertipSpecialBuffBlockView.Bind(base.ViewModel.OvertipSpecialBuffBlockVM);
		m_OvertipBuffsPredictionBlockView.Bind(base.ViewModel.OvertipBuffsPredictionBlockVM);
		m_OvertipConcentrationActionView.Bind(base.ViewModel.SurfaceCombatActionVM);
		m_DamageBlockPCView.Bind(base.ViewModel.DamageBlockVM);
		m_OvertipCoverBlockView.Bind(base.ViewModel.OvertipCoverBlockVM);
		m_HitChanceBlockPCView.Or(null)?.Bind(base.ViewModel.HitChanceBlockVM);
		CombatUnitInteractionView.Bind(base.ViewModel.CombatUnitInteractionVM);
	}

	private void HandleHovered(bool isHovered)
	{
		m_ScaleUpTween?.Pause();
		m_ScaleDownTween?.Pause();
		if (!base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue)
		{
			m_RectTransform.localScale = Vector3.one;
			return;
		}
		Tween t = ((!isHovered) ? (m_ScaleDownTween ?? (m_ScaleDownTween = m_RectTransform.DOScale(1f, 0.2f).SetUpdate(isIndependentUpdate: false).OnUpdate(m_CombatTextBlockPCView.UpdateVisualForCommon)
			.SetAutoKill(autoKillOnCompletion: false))) : (m_ScaleUpTween ?? (m_ScaleUpTween = m_RectTransform.DOScale(m_HoveredScale, 0.2f).SetUpdate(isIndependentUpdate: false).OnUpdate(m_CombatTextBlockPCView.UpdateVisualForCommon)
			.SetAutoKill(autoKillOnCompletion: false))));
		t.Restart();
	}

	private void DoDeath()
	{
		CombatSounds.Instance.Combat.UnitDeath.Play();
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

	private void UpdateOvertipHeight()
	{
		Vector2 sizeDelta = m_RectTransform.sizeDelta;
		float y = (base.ViewModel.MechanicEntityUIState.HasArmor.CurrentValue ? m_RectTransformHasArmorHeight : m_RectTransformNoArmorHeight);
		m_RectTransform.sizeDelta = new Vector2(sizeDelta.x, y);
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
}
