using System;
using System.Collections.Generic;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartQuickAccessConsoleView : View<ActionBarVM>, IUnitDirectHoverUIHandler, ISubscriber, IClickMechanicActionBarSlotHandler, IAbilityTargetSelectionUIHandler, IAbilityOwnerTargetSelectionHandler, ICullFocusHandler
{
	[SerializeField]
	private ActionBarQuickAccessCarouselView m_HorizontalCarousel;

	[SerializeField]
	private ActionBarQuickAccessCarouselView m_VerticalCarousel;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private ActionBarPartWeaponsConsoleView m_WeaponsConsoleView;

	[SerializeField]
	private ActionBarPartAbilitiesConsoleView m_AbilitiesConsoleView;

	[SerializeField]
	private RectTransform m_EmptyMoveSlot;

	[Header("Hints")]
	[SerializeField]
	private HintView m_DPadSurfaceHint;

	[SerializeField]
	private HintView m_DPadCombatHint;

	[SerializeField]
	private HintView m_DPadInternalHint;

	[SerializeField]
	private HintView m_ConfirmHint;

	[SerializeField]
	private HintView m_InfoHint;

	[SerializeField]
	private HintView m_AbilitiesHint;

	private IDisposable m_Disposable;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	private bool m_ShowTooltip;

	private readonly ReactiveProperty<bool> m_HasSlot = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanActivate = new ReactiveProperty<bool>();

	private IDisposable m_CanActivateSubscription;

	private ActionBarQuickAccessCarouselView m_ActiveCarouselView;

	public void Initialize()
	{
		m_MoveAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_HorizontalCarousel.Initialize(base.ViewModel.GetQuickAccessSlotVM()).AddTo(this);
		m_VerticalCarousel.Initialize(base.ViewModel.GetQuickAccessSlotVM()).AddTo(this);
		m_WeaponsConsoleView.Bind(base.ViewModel.Weapons);
		base.ViewModel.Weapons.CurrentSet.Subscribe(delegate(ActionBarPartWeaponSetVM set)
		{
			if (set != null)
			{
				m_HorizontalCarousel.SetSlots(set.AllSlots);
				m_Disposable?.Dispose();
				m_Disposable = ObservableSubscribeExtensions.Subscribe(set.SlotsUpdated, delegate
				{
					m_HorizontalCarousel.SetSlots(set.AllSlots);
				});
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Consumables.UnitChanged, delegate
		{
			OnDeclineClicked();
			m_VerticalCarousel.SetSlots(base.ViewModel.Consumables.Slots);
		}).AddTo(this);
		base.ViewModel.QuickAccessSlot.Subscribe(OnQuickAccessSlotChanged).AddTo(this);
		CreateInput();
		m_IsActive.Subscribe(Activate).AddTo(this);
		m_IsActive.CombineLatest(m_HasSlot, (bool isActive, bool hasSlot) => new { isActive, hasSlot }).Subscribe(value =>
		{
			m_EmptyMoveSlot.Or(null)?.gameObject.SetActive(!value.isActive && !value.hasSlot);
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_CanActivateSubscription?.Dispose();
		m_Disposable?.Dispose();
	}

	private void CreateInput()
	{
	}

	private void Activate(bool active)
	{
		if (active)
		{
			m_MoveAnimator.AppearAnimation();
			CombatSounds.Instance.ActionBar.DPadShow.Play();
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
			Game.Instance.CursorController.SetActive(value: true);
		}
		else
		{
			m_MoveAnimator.DisappearAnimation();
			CombatSounds.Instance.ActionBar.DPadHide.Play();
		}
	}

	private void OnQuickAccessSlotChanged(ActionBarSlotVM slotVM)
	{
		m_HasSlot.Value = slotVM != null;
		m_CanActivateSubscription?.Dispose();
		if (m_HasSlot.Value)
		{
			m_CanActivateSubscription = slotVM?.IsPossibleActive.Subscribe(delegate(bool a)
			{
				m_CanActivate.Value = a;
			});
		}
		else
		{
			m_CanActivate.Value = false;
		}
		if (m_ShowTooltip)
		{
			((MonoBehaviour)null).ShowTooltip(slotVM?.Tooltip?.CurrentValue, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2> { Vector2.zero }
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	public void AddInput()
	{
	}

	private void OnUpClicked()
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_VerticalCarousel.ClickNext();
	}

	private void OnDownClicked()
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_VerticalCarousel.ClickPrevious();
	}

	private void OnLeftClicked()
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_HorizontalCarousel.ClickPrevious();
	}

	private void OnRightClicked()
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_HorizontalCarousel.ClickNext();
	}

	private void OnConfirmClicked()
	{
		ButtonsSounds.Instance.Default.Click.Play();
		base.ViewModel.QuickAccessSlot.CurrentValue.OnMainClick();
	}

	private void CheckPingCoop()
	{
		if (m_CanActivate.Value)
		{
			return;
		}
		PhotonManager.Ping.CheckPingCoop(delegate
		{
			ActionBarSlotVM currentValue = base.ViewModel.QuickAccessSlot.CurrentValue;
			if (!string.IsNullOrWhiteSpace(currentValue.MechanicActionBarSlot.KeyName))
			{
				PhotonManager.Ping.PingActionBarAbility(currentValue.MechanicActionBarSlot.KeyName, currentValue.MechanicActionBarSlot.Unit, currentValue.Index);
			}
		});
	}

	private void OnDeclineClicked()
	{
		m_IsActive.Value = false;
		m_IsActive.Value = false;
		m_IsActive.Value = false;
	}

	private void OnAbilitiesActivate()
	{
		OnDeclineClicked();
		m_AbilitiesConsoleView.Activate();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if ((bool)SettingsRoot.Game.TurnBased.AutoSelectWeaponAbility && Game.Instance.Player.IsInCombat && !m_HorizontalCarousel.IsActive.CurrentValue && !m_VerticalCarousel.IsActive.CurrentValue && !RootUIContext.Instance.IsInitiativeTrackerActive)
		{
			if (!isHover)
			{
				base.ViewModel.ClearQuickAccessSlot();
			}
			else if ((!Game.Instance.Player.IsInCombat || Game.Instance.Controllers.TurnController.IsPlayerTurn) && !(Game.Instance.Controllers.SelectedAbilityHandler?.Ability != null))
			{
				base.ViewModel.SetSuitableQuickAccessSlot(unitEntityView);
			}
		}
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		m_IsActive.Value = false;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_IsActive.Value = false;
		m_IsActive.Value = false;
	}

	public void HandleOwnerAbilitySelected(AbilityData ability)
	{
		m_IsActive.Value = false;
		m_IsActive.Value = false;
	}

	public void HandleRemoveFocus()
	{
		if (m_HorizontalCarousel.IsActive.CurrentValue)
		{
			m_ActiveCarouselView = m_HorizontalCarousel;
		}
		else if (m_VerticalCarousel.IsActive.CurrentValue)
		{
			m_ActiveCarouselView = m_VerticalCarousel;
		}
		if (m_ActiveCarouselView != null)
		{
			m_ActiveCarouselView.HandleFocusState(shouldShowFocus: false);
		}
	}

	public void HandleRestoreFocus()
	{
		if (m_ActiveCarouselView != null)
		{
			m_ActiveCarouselView.HandleFocusState(shouldShowFocus: true);
		}
		m_ActiveCarouselView = null;
	}
}
