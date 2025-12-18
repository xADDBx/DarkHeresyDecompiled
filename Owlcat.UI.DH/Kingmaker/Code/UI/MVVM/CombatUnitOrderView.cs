using System;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatUnitOrderView : View<InitiativeTrackerMechanicEntityVM>, ITurnVirtualItemView, IUnitHighlightUIHandler, ISubscriber, IInteractionHighlightUIHandler
{
	[Serializable]
	private struct ViewVariant
	{
		public GameObject Root;

		public RectTransform RootTransform;

		public CombatUnitEntityView EntityView;

		public CombatUnitDelayedActionView DelayedActionView;
	}

	[SerializeField]
	private RectTransform m_RootTransform;

	[SerializeField]
	private CanvasGroup m_RootCanvasGroup;

	[SerializeField]
	private ViewVariant m_PartyUnitVariant;

	[SerializeField]
	private ViewVariant m_DefaultVariant;

	[SerializeField]
	private InitiativeTrackerRoundWidget m_RoundWidget;

	[Header("Animation settings")]
	[SerializeField]
	protected float m_AnimationTime = 0.3f;

	[HideInInspector]
	public bool WillBeDestroyedExternal;

	private readonly ReactiveProperty<bool> m_IsTargetSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsHPZoneVisible = new ReactiveProperty<bool>();

	public MonoBehaviour View => this;

	public OwlcatSelectable Selectable => null;

	public RectTransform RectTransform => m_RootTransform;

	public CanvasGroup CanvasGroup => m_RootCanvasGroup;

	public RectTransform UnitNameAnchor { get; private set; }

	public bool WillBeReused { get; set; }

	public Tween GetHideAnimation(Action completeAction)
	{
		return GetHideAnimationInternal(completeAction);
	}

	public Tween GetMoveAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetMoveAnimationInternal(completeAction, targetPosition);
	}

	public Tween GetShowAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetShowAnimationInternal(completeAction, targetPosition);
	}

	public void SetAnchoredPosition(Vector2 position)
	{
		RectTransform.anchoredPosition = position;
	}

	public ViewModel GetViewModel()
	{
		return base.ViewModel;
	}

	public void ViewBind(ITurnVirtualItemData data)
	{
		SetAlphaAndScale();
		Bind((InitiativeTrackerMechanicEntityVM)(data?.ViewModel));
		WillBeDestroyedExternal = true;
	}

	public void DestroyViewItem()
	{
		OnUnbind();
	}

	public void HandleHighlightChange(AbstractUnitEntityView unit)
	{
		if (unit != base.ViewModel?.MechanicEntity?.View)
		{
			DelayedInvoker.InvokeInTime(delegate
			{
				TryHandleHighlightChangeForSquadUnit(unit);
			}, 0.1f);
		}
		else
		{
			UpdateHighlightedState();
		}
	}

	public virtual void HandleHighlightChange(bool isOn)
	{
		UpdateHighlightedState();
	}

	public void TryHandleHighlightChangeForSquadUnit(AbstractUnitEntityView unit)
	{
		if (base.ViewModel == null || !base.ViewModel.IsSquadLeader.CurrentValue || base.ViewModel.NeedToShow.CurrentValue)
		{
			return;
		}
		UnitSquad unitSquad = base.ViewModel?.Squad;
		if (unitSquad == null)
		{
			return;
		}
		foreach (UnitReference unit2 in unitSquad.Units)
		{
			BaseUnitEntity baseUnitEntity = unit2.ToBaseUnitEntity();
			if (!(baseUnitEntity.View != unit))
			{
				UpdateHighlightedStateForSquadUnit(baseUnitEntity);
				break;
			}
		}
	}

	public Vector2 GetSizeWithPortrait(bool isPartyUnit)
	{
		ViewVariant obj = (isPartyUnit ? m_PartyUnitVariant : m_DefaultVariant);
		return obj.RootTransform.sizeDelta;
	}

	public Vector2 GetSizeRound()
	{
		return m_RoundWidget.GetSize();
	}

	protected override void OnBind()
	{
		m_PartyUnitVariant.Root.SetActive(value: false);
		m_DefaultVariant.Root.SetActive(value: false);
		m_RoundWidget.SetActive(isActive: false);
		UnitNameAnchor = null;
		if (base.ViewModel.HasUnit)
		{
			WillBeDestroyedExternal = true;
			base.ViewModel.MechanicEntityUIState.IsAoETarget.CombineLatest(base.ViewModel.MechanicEntityUIState.Ability, base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.IsTargetSelection, base.ViewModel.IsCurrent, base.ViewModel.MechanicEntityUIState.IsPingUnit, (bool _, AbilityData _, bool _, bool _, bool _, bool _) => true).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
			{
				UpdateHighlightedState();
			})
				.AddTo(this);
			ViewVariant viewVariant = (base.ViewModel.IsPlayer.CurrentValue ? m_PartyUnitVariant : m_DefaultVariant);
			viewVariant.EntityView.Bind(base.ViewModel);
			viewVariant.DelayedActionView.Bind(base.ViewModel);
			viewVariant.Root.SetActive(value: true);
			UnitNameAnchor = (base.ViewModel.IsInitiativeHolder ? viewVariant.DelayedActionView.UnitNameAnchor : viewVariant.EntityView.UnitNameAnchor);
			SetupOwnSize(viewVariant.RootTransform.sizeDelta);
		}
		else
		{
			base.ViewModel.Round.Subscribe(m_RoundWidget.SetRoundText).AddTo(this);
			m_RoundWidget.SetActive(isActive: true);
			SetupOwnSize(m_RoundWidget.GetSize());
			base.transform.SetAsFirstSibling();
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		if (!WillBeDestroyedExternal)
		{
			base.ViewModel.InvokeUnitViewHighlight(state: false);
		}
	}

	protected abstract Tween GetHideAnimationInternal(Action completeAction);

	protected abstract Tween GetMoveAnimationInternal(Action completeAction, Vector2 targetPosition);

	protected abstract Tween GetShowAnimationInternal(Action completeAction, Vector2 targetPosition);

	private void UpdateHighlightedState()
	{
		if (!(base.ViewModel.MechanicEntity?.View == null))
		{
			m_IsTargetSelected.Value = base.ViewModel.MechanicEntityUIState.IsAoETarget.CurrentValue || base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue;
			m_IsHPZoneVisible.Value = base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue || base.ViewModel.MechanicEntityUIState.IsPingUnit.CurrentValue;
		}
	}

	private void SetAlphaAndScale()
	{
		CanvasGroup.alpha = 0f;
		RectTransform.localScale = new Vector3(1f, 1f, 1f);
	}

	private void SetupOwnSize(Vector2 size)
	{
		RectTransform.sizeDelta = size;
	}

	private void UpdateHighlightedStateForSquadUnit(BaseUnitEntity unit)
	{
		MechanicEntityUIState orCreateUnitState = UnitUIStateHolder.Instance.GetOrCreateUnitState(unit);
		m_IsTargetSelected.Value = orCreateUnitState.IsAoETarget.CurrentValue || (base.ViewModel.IsTargetSelection.CurrentValue && orCreateUnitState.IsMouseOverUnit.CurrentValue);
		m_IsHPZoneVisible.Value = unit.View.IsHighlighted || orCreateUnitState.IsAoETarget.CurrentValue || orCreateUnitState.IsMouseOverUnit.CurrentValue || orCreateUnitState.IsPingUnit.CurrentValue;
	}
}
