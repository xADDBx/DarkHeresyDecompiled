using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootConsoleView : LootView<LootCollectorConsoleView, InteractionSlotPartConsoleView, PlayerStashConsoleView>, ICullFocusHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CargoTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[SerializeField]
	private CanvasSortingComponent m_SortingComponent;

	[SerializeField]
	private ConsoleHintsWidget m_MiddleHintsWidget;

	[SerializeField]
	private ConsoleHintsWidget m_RightHintsWidget;

	[SerializeField]
	protected RectTransform m_LeftCanvas;

	[SerializeField]
	protected RectTransform m_RightCanvas;

	[SerializeField]
	protected RectTransform m_CenterCanvas;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_FocusedRotation = 5f;

	[SerializeField]
	private float m_FocusTweenTime = 0.5f;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasItem = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanTransfer = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_InventoryFocus = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_LootFocus = new ReactiveProperty<bool>();

	private IConsoleEntity m_CurrentEntity;

	private IConsoleHint m_MiddleConfirmHint;

	private IConsoleHint m_RightConfirmHint;

	private ItemSlotsGroupType m_LastFocusGroup = ItemSlotsGroupType.Loot;

	private Vector3 m_LeftCanvasInitPosition;

	private Vector3 m_RightCanvasInitPosition;

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private IDisposable m_UpdateNavigationDelay;

	private IConsoleEntity m_CulledFocus;

	protected override void OnBind()
	{
		base.OnBind();
		m_LeftCanvasInitPosition = m_LeftCanvas.anchoredPosition;
		m_RightCanvasInitPosition = m_RightCanvas.anchoredPosition;
		m_MiddleHintsWidget.gameObject.SetActive(value: true);
		m_RightHintsWidget.gameObject.SetActive(value: true);
		m_LootFocus.Skip(1).Subscribe(OnFocusToPanelCenter).AddTo(this);
		m_InventoryFocus.Skip(1).Subscribe(OnFocusToPanelRight).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		CreateNavigationDelayed();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		DOTween.Kill(m_LeftCanvas);
		DOTween.Kill(m_RightCanvas);
		DOTween.Kill(m_CenterCanvas);
		m_LeftCanvas.anchoredPosition = m_LeftCanvasInitPosition;
		m_RightCanvas.anchoredPosition = m_RightCanvasInitPosition;
		TooltipHelper.HideTooltip();
	}

	private void CreateNavigationDelayed()
	{
		DelayedInvoker.InvokeInFrames(CreateNavigation, 2);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(base.ViewModel.LootUpdated, delegate
		{
			OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
		}).AddTo(this);
		AddNavigation();
		CreateInput();
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused).AddTo(this);
	}

	private void AddNavigation()
	{
		IConsoleEntity consoleEntity = null;
		if (m_LootFocus.Value)
		{
			consoleEntity = m_CollectorExitLocation.GetCurrentFocus();
		}
		m_NavigationBehaviour.Clear();
		ConsoleNavigationBehaviour consoleNavigationBehaviour = null;
		ConsoleNavigationBehaviour consoleNavigationBehaviour2 = null;
		consoleNavigationBehaviour = m_CollectorExitLocation.GetNavigation();
		m_NavigationBehaviour.AddEntityHorizontal(consoleNavigationBehaviour);
		LootWindowMode mode = base.ViewModel.Mode;
		if (mode != LootWindowMode.PlayerChest && mode != LootWindowMode.OneSlot)
		{
			_ = base.ViewModel.LootCollectorExitLocation.Loot.ExtendedView.CurrentValue;
		}
		IConsoleEntity consoleEntity2 = null;
		IConsoleEntity consoleEntity3 = consoleNavigationBehaviour?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
		switch (m_LastFocusGroup)
		{
		case ItemSlotsGroupType.Inventory:
			consoleEntity2 = ((consoleNavigationBehaviour2 != null) ? consoleNavigationBehaviour2.Entities.FirstOrDefault((IConsoleEntity e) => e.IsValid()) : consoleEntity3);
			break;
		case ItemSlotsGroupType.Loot:
			if (base.ViewModel.IsOneSlot)
			{
				consoleEntity2 = consoleNavigationBehaviour2?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
			}
			else
			{
				m_NavigationBehaviour.SetCurrentEntity(consoleNavigationBehaviour);
				consoleEntity2 = consoleEntity3;
			}
			break;
		case ItemSlotsGroupType.Unknown:
			if (base.ViewModel.IsOneSlot)
			{
				consoleEntity2 = consoleNavigationBehaviour2?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
			}
			break;
		}
		m_NavigationBehaviour.FocusOnEntityManual(consoleEntity ?? consoleEntity2);
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "LootConsoleView"
		});
		m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9).AddTo(this);
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip.And(m_LootFocus).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased);
		m_MiddleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct.AddTo(this);
		if (!base.ViewModel.IsOneSlot)
		{
			InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ShowContextMenu, 11, m_HasItem.And(m_LootFocus).ToReadOnlyReactiveProperty(initialValue: false));
			m_MiddleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.ContextMenu.ContextMenu).AddTo(this);
			inputBindStruct2.AddTo(this);
		}
		ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_CanTransfer.Select((bool value) => value || base.ViewModel.IsPlayerStash).And(m_LootFocus).ToReadOnlyReactiveProperty(initialValue: false);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
		}, 8, readOnlyReactiveProperty);
		m_MiddleConfirmHint = m_MiddleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ActionTexts.MoveItem).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip.And(m_InventoryFocus).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased);
		m_RightHintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		inputBindStruct4.AddTo(this);
		if (!base.ViewModel.IsOneSlot)
		{
			InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(ShowContextMenu, 11, m_HasItem.And(m_InventoryFocus).ToReadOnlyReactiveProperty(initialValue: false));
			m_RightHintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ContextMenu.ContextMenu).AddTo(this);
			inputBindStruct5.AddTo(this);
		}
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanTransfer.And(m_InventoryFocus).ToReadOnlyReactiveProperty(initialValue: false));
		m_RightConfirmHint = m_RightHintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.ActionTexts.MoveItem).AddTo(this);
		inputBindStruct6.AddTo(this);
		if (!base.ViewModel.IsOneSlot && !base.ViewModel.IsPlayerStash)
		{
			m_CollectorExitLocation.AddInput(m_InputLayer);
		}
		m_SortingComponent.PushView().AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void ShowContextMenu(InputActionEventData data)
	{
		ReadOnlyReactiveProperty<List<ContextMenuCollectionEntity>> readOnlyReactiveProperty = (m_CurrentEntity as IItemSlotView)?.SlotVM?.ContextMenu;
		if (readOnlyReactiveProperty != null)
		{
			((m_CurrentEntity as MonoBehaviour) ?? (m_CurrentEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(readOnlyReactiveProperty.CurrentValue);
		}
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
		}
		else
		{
			base.ViewModel.Close();
		}
	}

	private void UpdateNavigationDelayed()
	{
		m_UpdateNavigationDelay?.Dispose();
		m_UpdateNavigationDelay = DelayedInvoker.InvokeInFrames(UpdateNavigation, 3);
	}

	private void UpdateNavigation()
	{
		AddNavigation();
	}

	private void OnFocusToPanelLeft(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: true);
			SetRightPanelFocusState(state: false);
		}
	}

	private void OnFocusToPanelRight(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: false);
			SetRightPanelFocusState(state: true);
		}
	}

	private void OnFocusToPanelCenter(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: false);
			SetRightPanelFocusState(state: false);
		}
	}

	private void SetLeftPanelFocusState(bool state)
	{
		Vector3 endValue = (state ? new Vector3(0f, m_FocusedRotation, 0f) : Vector3.zero);
		m_LeftCanvas.DOLocalRotate(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetRightPanelFocusState(bool state)
	{
		Vector3 endValue = (state ? new Vector3(0f, 0f - m_FocusedRotation, 0f) : Vector3.zero);
		m_RightCanvas.DOLocalRotate(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		SetTooltip(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnEntityFocused(IConsoleEntity entity)
	{
	}

	private void DefineFocusPanel(IConsoleEntity entity)
	{
		if ((entity as IItemSlotView)?.SlotVM != null)
		{
			ItemSlotVM itemSlotVM = (entity as IItemSlotView)?.SlotVM;
			m_InventoryFocus.Value = (itemSlotVM != null && itemSlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory) || entity is InsertableLootSlotConsoleView;
			m_LootFocus.Value = itemSlotVM != null && itemSlotVM.SlotsGroupType == ItemSlotsGroupType.Loot;
		}
		else if ((entity as VirtualListElement)?.Data != null)
		{
			_ = (entity as VirtualListElement).Data;
			m_InventoryFocus.Value = false;
			m_LootFocus.Value = false;
		}
	}

	private void SetConfirmLabel(IConsoleEntity entity)
	{
		string label = string.Empty;
		if (base.ViewModel.IsOneSlot)
		{
			if (!(entity is InsertableLootSlotView))
			{
				if (entity is LootSlotConsoleView)
				{
					label = UIStrings.Instance.ContextMenu.TakeOff.Text;
				}
			}
			else
			{
				label = UIStrings.Instance.ContextMenu.Use.Text;
			}
		}
		else if (base.ViewModel.IsPlayerStash)
		{
			if (!(entity is InventorySlotConsoleView))
			{
				if (entity is LootSlotConsoleView)
				{
					label = UIStrings.Instance.LootWindow.SendToInventory.Text;
				}
			}
			else
			{
				label = UIStrings.Instance.LootWindow.SendToPlayerChest.Text;
			}
		}
		else if (!(entity is InventorySlotConsoleView))
		{
			if (entity is LootSlotConsoleView)
			{
				label = UIStrings.Instance.LootWindow.SendToInventory.Text;
			}
		}
		else
		{
			label = UIStrings.Instance.LootWindow.SendToInventory.Text;
		}
		m_MiddleConfirmHint?.SetLabel(label);
		m_RightConfirmHint?.SetLabel(label);
	}

	private void SetTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		UpdateTooltipConfigs(entity);
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = !list.Empty();
			m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowComparativeTooltip(list, m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs(IConsoleEntity currentEntity)
	{
		if (currentEntity is IItemSlotView)
		{
			TooltipPlaces tooltipPlaces = (m_InventoryFocus.Value ? m_StashTooltipPlaces : m_CenterTooltipPlaces);
			m_MainTooltipConfig = tooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
			m_CompareTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
		}
		else
		{
			m_MainTooltipConfig = new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None);
		}
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}
}
