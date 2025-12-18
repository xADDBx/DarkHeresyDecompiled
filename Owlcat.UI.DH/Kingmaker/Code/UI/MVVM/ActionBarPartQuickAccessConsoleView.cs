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
using Rewired;
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
	private ConsoleHint m_DPadSurfaceHint;

	[SerializeField]
	private ConsoleHint m_DPadCombatHint;

	[SerializeField]
	private ConsoleHint m_DPadInternalHint;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_InfoHint;

	[SerializeField]
	private ConsoleHint m_AbilitiesHint;

	private IDisposable m_Disposable;

	private InputLayer m_InputLayer;

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
			OnDeclineClicked(default(InputActionEventData));
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
		m_InputLayer = null;
		m_CanActivateSubscription?.Dispose();
		m_Disposable?.Dispose();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "SurfaceActionBarPartQuickAccessConsoleView"
		};
		m_DPadInternalHint.BindCustomAction(new List<int> { 6, 7, 4, 5 }, m_InputLayer).AddTo(this);
		m_InputLayer.AddButton(OnUpClicked, 6, m_VerticalCarousel.HasSlots).AddTo(this);
		m_InputLayer.AddButton(OnDownClicked, 7, m_VerticalCarousel.HasSlots).AddTo(this);
		m_InputLayer.AddButton(OnLeftClicked, 4, m_HorizontalCarousel.HasSlots).AddTo(this);
		m_InputLayer.AddButton(OnRightClicked, 5, m_HorizontalCarousel.HasSlots).AddTo(this);
		m_InputLayer.AddAxis2D(OnLeftStickMoved, 0, 1, repeat: false).AddTo(this);
		m_InputLayer.AddAxis2D(OnRightStickMoved, 2, 3, repeat: false).AddTo(this);
		m_InfoHint.Bind(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasSlot, InputActionEventType.ButtonJustReleased)).AddTo(this);
		m_InputLayer.AddButton(OnDeclineClicked, 9).AddTo(this);
		m_ConfirmHint.Bind(m_InputLayer.AddButton(OnConfirmClicked, 8, m_CanActivate)).AddTo(this);
		m_InputLayer.AddButton(CheckPingCoop, 8, m_CanActivate.Not().ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		m_AbilitiesHint.Bind(m_InputLayer.AddButton(OnAbilitiesActivate, 11)).AddTo(this);
		m_WeaponsConsoleView.AddInput(m_InputLayer);
	}

	private void Activate(bool active)
	{
		if (active)
		{
			m_MoveAnimator.AppearAnimation();
			UISounds.Instance.Sounds.ActionBar.DPadShow.Play();
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
			GamePad.Instance.PushLayer(m_InputLayer);
			Game.Instance.CursorController.SetActive(active: true);
		}
		else
		{
			m_MoveAnimator.DisappearAnimation();
			UISounds.Instance.Sounds.ActionBar.DPadHide.Play();
			GamePad.Instance.PopLayer(m_InputLayer);
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

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnQuickAccessSlotChanged(base.ViewModel.QuickAccessSlot.CurrentValue);
	}

	public void AddInput(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enable, bool inCombat)
	{
		(inCombat ? m_DPadCombatHint : m_DPadSurfaceHint).BindCustomAction(new List<int> { 6, 7, 4, 5 }, inputLayer).AddTo(this);
		inputLayer.AddButton(OnUpClicked, 6, enable).AddTo(this);
		inputLayer.AddButton(OnDownClicked, 7, enable, InputActionEventType.ButtonJustReleased).AddTo(this);
		inputLayer.AddButton(OnLeftClicked, 4, enable).AddTo(this);
		inputLayer.AddButton(OnRightClicked, 5, enable).AddTo(this);
		m_WeaponsConsoleView.AddInput(inputLayer);
	}

	private void OnUpClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_VerticalCarousel.ClickNext();
	}

	private void OnDownClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_VerticalCarousel.ClickPrevious();
	}

	private void OnLeftClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_HorizontalCarousel.ClickPrevious();
	}

	private void OnRightClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_IsActive.Value = false;
		m_IsActive.Value = true;
		m_HorizontalCarousel.ClickNext();
	}

	private void OnLeftStickMoved(InputActionEventData data, Vector2 vector)
	{
		SurfaceMainInputLayer.MoveCursor(vector);
	}

	private void OnRightStickMoved(InputActionEventData data, Vector2 vector)
	{
		SurfaceMainInputLayer.MoveRotateCamera(vector);
	}

	private void OnConfirmClicked(InputActionEventData data)
	{
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
		base.ViewModel.QuickAccessSlot.CurrentValue.OnMainClick();
	}

	private void CheckPingCoop(InputActionEventData data)
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

	private void OnDeclineClicked(InputActionEventData data)
	{
		m_IsActive.Value = false;
		m_IsActive.Value = false;
		m_IsActive.Value = false;
	}

	private void OnAbilitiesActivate(InputActionEventData data)
	{
		OnDeclineClicked(data);
		m_AbilitiesConsoleView.Activate();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if ((bool)SettingsRoot.Game.TurnBased.AutoSelectWeaponAbility && (Game.Instance.Player.IsInCombat || GamePad.Instance.CursorEnabled) && !m_HorizontalCarousel.IsActive.CurrentValue && !m_VerticalCarousel.IsActive.CurrentValue && !RootUIContext.Instance.IsInitiativeTrackerActive)
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
