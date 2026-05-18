using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryConsoleView : InventoryBaseView<InventoryEquipSlotConsoleView>, IInventoryHandler, ISubscriber, IEquipItemAutomaticallyHandler, IContextMenuHandler, ISplitItemHandler, ICounterWindowUIHandler, IInsertItemHandler
{
	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private RectTransform m_TooltipPlaceCenter;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

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

	private InventoryStashConsoleView StashConsoleView => m_StashView as InventoryStashConsoleView;

	private InventoryDollConsoleView DollConsoleView => m_DollView as InventoryDollConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.DollVM.ChooseSlotMode.Subscribe(SetBusyTooltipMode).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ShowTooltip.Value = false;
	}

	private void CreateInput()
	{
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

	private void ShowContextMenu()
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
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
	}

	public void HandleEquipItemAutomatically(ItemEntity item)
	{
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
	}
}
