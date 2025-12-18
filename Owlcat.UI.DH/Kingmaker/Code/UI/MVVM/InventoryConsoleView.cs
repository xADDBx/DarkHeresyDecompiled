using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryConsoleView : InventoryBaseView<InventoryEquipSlotConsoleView>, IInventoryHandler, ISubscriber, IEquipItemAutomaticallyHandler, ICullFocusHandler, IContextMenuHandler, ISplitItemHandler, ICounterWindowUIHandler, IInsertItemHandler
{
	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private RectTransform m_TooltipPlaceCenter;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_FocusedRotation = 10f;

	[SerializeField]
	private float m_FocusedOffsetX = 10f;

	[SerializeField]
	private float m_FocusTweenTime = 0.5f;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_NavigationPanelLeft;

	private GridConsoleNavigationBehaviour m_NavigationPanelCenter;

	private GridConsoleNavigationBehaviour m_NavigationPanelRight;

	private readonly ReactiveProperty<bool> m_FocusOnRightPanel = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasContextMenu = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanEquip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanChoose = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSelect = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanFuncAdd = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<IItemSlotView> m_CurrentItemSlot = new ReactiveProperty<IItemSlotView>();

	private readonly CompositeDisposable m_FocusDisposable = new CompositeDisposable();

	private Vector3 m_LeftCanvasInitPosition;

	private Vector3 m_RightCanvasInitPosition;

	private bool m_ShowTooltipPrevValue;

	private IConsoleHint m_FuncAddHint;

	private IConsoleHint m_TooltipHint;

	private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_StatsTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.LongRightStickButton
	};

	private IConsoleEntity m_CulledFocus;

	private InventoryStashConsoleView StashConsoleView => m_StashView as InventoryStashConsoleView;

	private InventoryDollConsoleView DollConsoleView => m_DollView as InventoryDollConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
		base.ViewModel.Unit.Skip(1).Subscribe(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}).AddTo(this);
		base.ViewModel.DollVM.InventorySelectorWindowVM.Subscribe(delegate(InventorySelectorWindowVM value)
		{
			SetBusyTooltipMode(value != null);
			if (value == null)
			{
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
		}).AddTo(this);
		base.ViewModel.DollVM.ChooseSlotMode.Subscribe(SetBusyTooltipMode).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ShowTooltip.Value = false;
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "InventoryConsoleView"
		});
		GridConsoleNavigationBehaviour navigation = StashConsoleView.GetNavigation();
		m_NavigationPanelLeft = new GridConsoleNavigationBehaviour().AddTo(this);
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_NavigationPanelLeft.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour);
		gridConsoleNavigationBehaviour.FocusOnFirstValidEntity();
		m_NavigationPanelLeft.FocusOnEntityManual(gridConsoleNavigationBehaviour);
		m_NavigationPanelCenter = DollConsoleView.GetNavigation().AddTo(this);
		m_NavigationPanelRight = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationPanelRight = navigation;
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelLeft);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelCenter);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelRight);
		m_NavigationPanelLeft.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelLeft).AddTo(this);
		m_NavigationPanelCenter.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelCenter).AddTo(this);
		m_NavigationPanelRight.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelRight).AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity).AddTo(this);
		base.ViewModel.StashVM.CurrentFilter.Subscribe(delegate
		{
			if (m_NavigationBehaviour.Focus.Value == m_NavigationPanelRight)
			{
				m_NavigationPanelRight.FocusOnFirstValidEntity();
				m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight.DeepestNestedFocus);
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
			else
			{
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
		}).AddTo(this);
		base.ViewModel.StashVM.CurrentSorter.Subscribe(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}).AddTo(this);
		CreateInput();
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_NavigationPanelRight.FocusOnFirstValidEntity();
			m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight.DeepestNestedFocus);
		}).AddTo(this);
	}

	private void CreateInput()
	{
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9);
		m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		m_TooltipHint = m_ConsoleHintsWidget.BindHint(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
		m_TooltipHint.SetLabel(UIStrings.Instance.CommonTexts.Information);
		StashConsoleView.ItemsFilter.AddInput(m_InputLayer, m_FocusOnRightPanel, m_ConsoleHintsWidget);
		DollConsoleView.AddInput(m_InputLayer, m_ConsoleHintsWidget, null, m_ShowTooltip);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu, InputActionEventType.ButtonJustReleased);
		m_ConsoleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ContextMenu.ContextMenu).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(delegate
		{
			OnFuncAdditionalClick();
		}, 17, m_CanFuncAdd);
		m_FuncAddHint = m_ConsoleHintsWidget.BindHint(inputBindStruct4).AddTo(this);
		inputBindStruct4.AddTo(this);
		InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanEquip, InputActionEventType.ButtonJustReleased);
		m_ConsoleHintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ContextMenu.Equip).AddTo(this);
		inputBindStruct5.AddTo(this);
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanChoose, InputActionEventType.ButtonJustReleased);
		m_ConsoleHintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.InventoryScreen.ChooseItem).AddTo(this);
		inputBindStruct6.AddTo(this);
		InputBindStruct inputBindStruct7 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSelect, InputActionEventType.ButtonJustReleased);
		m_ConsoleHintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.CommonTexts.Select).AddTo(this);
		inputBindStruct7.AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_CanvasSortingComponent.PushView().AddTo(this);
	}

	private void OnFocusToPanelLeft(IConsoleEntity entity)
	{
		if (entity != null && m_FocusOnRightPanel.Value)
		{
			m_FocusOnRightPanel.Value = false;
		}
	}

	private void OnFocusToPanelRight(IConsoleEntity entity)
	{
		if (entity != null && !m_FocusOnRightPanel.Value)
		{
			m_FocusOnRightPanel.Value = true;
		}
	}

	private void OnFocusToPanelCenter(IConsoleEntity entity)
	{
		if (entity != null)
		{
			if (DollConsoleView.IsFocusOnRightSlots(entity))
			{
				OnFocusToPanelRight(entity);
			}
			else
			{
				OnFocusToPanelLeft(entity);
			}
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_FocusDisposable.Clear();
		UpdateHintsValues(entity);
		UpdateTooltip(entity);
	}

	private void UpdateTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		UpdateTooltipConfigs();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			TooltipConfig config = (m_NavigationPanelLeft.IsFocused ? m_StatsTooltipConfig : m_MainTooltipConfig);
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, config, shouldNotHideLittleTooltip: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = list.Count > 0;
			if (m_HasTooltip.Value && m_ShowTooltip.Value)
			{
				m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
				monoBehaviour.ShowComparativeTooltip(hasTooltipTemplates.TooltipTemplates(), m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs()
	{
		if (m_FocusOnRightPanel.Value && (bool)m_StashTooltipPlaces)
		{
			m_MainTooltipConfig = m_StashTooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
			m_CompareTooltipConfig = m_StashTooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
		}
		else
		{
			m_MainTooltipConfig.TooltipPlace = m_TooltipPlaceCenter;
			m_MainTooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0.5f)
			};
		}
	}

	private void UpdateHintsValues(IConsoleEntity entity)
	{
		m_CurrentItemSlot.Value = entity as IItemSlotView;
		bool value = m_CurrentItemSlot.Value != null && (m_CurrentItemSlot.Value.SlotVM?.ContextMenu?.CurrentValue.Any((ContextMenuCollectionEntity item) => item.IsEnabled)).GetValueOrDefault();
		m_HasContextMenu.Value = value;
		m_CanEquip.Value = (m_CurrentItemSlot.Value?.SlotVM?.IsEquipPossible).GetValueOrDefault() && !DollConsoleView.IsSlot(entity) && base.ViewModel.Unit.CurrentValue.CanBeControlled();
		m_CanChoose.Value = DollConsoleView.IsSlot(entity);
		m_CanSelect.Value = m_NavigationPanelLeft.IsFocused && ((entity as IConfirmClickHandler)?.CanConfirmClick() ?? false);
		if (entity is IFuncAdditionalClickHandler funcAdditionalClickHandler)
		{
			m_CanFuncAdd.Value = funcAdditionalClickHandler.CanFuncAdditionalClick();
			m_FuncAddHint.SetLabel(funcAdditionalClickHandler.GetFuncAdditionalClickHint());
		}
		else
		{
			m_CanFuncAdd.Value = false;
		}
	}

	private void ShowContextMenu(InputActionEventData obj)
	{
		if (m_CurrentItemSlot.Value is IConsoleEntity consoleEntity)
		{
			((consoleEntity as MonoBehaviour) ?? (consoleEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(m_CurrentItemSlot.Value.SlotVM?.ContextMenu?.CurrentValue);
		}
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
			return;
		}
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	void IInventoryHandler.Refresh()
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
	}

	public void HandleEquipItemAutomatically(ItemEntity item)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
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

	private void SetBusyTooltipMode(bool isBusy)
	{
		if (isBusy)
		{
			m_ShowTooltipPrevValue = m_ShowTooltip.Value;
			m_ShowTooltip.Value = false;
			TooltipHelper.HideTooltip();
		}
		else
		{
			m_ShowTooltip.Value = m_ShowTooltipPrevValue;
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}
	}

	public void HandleContextMenuRequest(IContextMenuCollection collection)
	{
		SetBusyTooltipMode(collection != null);
	}

	public void HandleSplitItem()
	{
	}

	public void HandleAfterSplitItem(ItemEntity item)
	{
		SetBusyTooltipMode(isBusy: false);
	}

	public void HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
	}

	public void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command)
	{
		SetBusyTooltipMode(isBusy: true);
	}

	public void HandleCloseCounterWindow()
	{
	}

	public void HandleInsertItem(ItemSlot slot)
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}).AddTo(this);
	}

	private void OnFuncAdditionalClick()
	{
		(m_NavigationBehaviour.DeepestNestedFocus as IFuncAdditionalClickHandler)?.OnFuncAdditionalClick();
	}
}
