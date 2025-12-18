using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorVM : ViewModel, INewSlotsHandler, ISubscriber, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IVendorDealHandler, IMoveItemHandler, IInventoryHandler
{
	private readonly ReactiveProperty<VendorWindowsTab> m_ActiveTab = new ReactiveProperty<VendorWindowsTab>();

	public readonly InventoryStashVM StashVM;

	public readonly LensSelectorVM Selector;

	public readonly VendorTabNavigationVM VendorTabNavigationVM;

	public VendorTradePartVM VendorTradePartVM;

	public FactionReputationVM FactionReputationVM;

	private TradeLogic Vendor => VendorHelper.TradeLogic;

	public ReadOnlyReactiveProperty<VendorWindowsTab> ActiveTab => m_ActiveTab;

	public VendorVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Vendor);
		});
		ItemsSortingContext sortingCtx = new ItemsSortingContext
		{
			SorterType = Game.Instance.Player.UISettings.InventorySorter,
			FilterType = Game.Instance.Player.UISettings.InventoryFilter
		};
		ItemSlotsGroupVM itemSlotsGroupVM = new ItemSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, 5, 100, sortingCtx, Game.Instance.Player.UISettings.ShowUnavailableItems, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory);
		itemSlotsGroupVM.AddTo(this);
		StashVM = new InventoryStashVM(itemSlotsGroupVM).AddTo(this);
		VendorTabNavigationVM = new VendorTabNavigationVM().AddTo(this);
		VendorTabNavigationVM.ActiveTab.Subscribe(SelectWindow).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
	}

	protected override void OnDispose()
	{
		VendorTradePartVM?.Dispose();
		FactionReputationVM?.Dispose();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Vendor);
		});
	}

	private void SelectWindow(VendorWindowsTab tab)
	{
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			VendorTradePartVM?.Dispose();
			FactionReputationVM?.Dispose();
			VendorTradePartVM = new VendorTradePartVM();
			break;
		case VendorWindowsTab.Reputation:
			VendorTradePartVM?.Dispose();
			FactionReputationVM = new FactionReputationVM();
			break;
		}
		m_ActiveTab.Value = tab;
	}

	private void UpdatePlayerSide()
	{
		StashVM.CollectionChanged();
	}

	public void Close()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null)
		{
			_ = loadedAreaState.Settings.CapitalPartyMode;
			if (true && Game.Instance.LoadedAreaState.Settings.CapitalPartyMode && !UtilityNet.IsControlMainCharacterWithWarning())
			{
				return;
			}
		}
		if (Vendor.IsChanged)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.Vendor.BeforeClose, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					Game.Instance.GameCommandQueue.EndTrading();
				}
			});
		}
		else
		{
			Game.Instance.GameCommandQueue.EndTrading();
		}
	}

	void INewSlotsHandler.HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		UIInventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.IsInVendor)
		{
			from.VendorTryMove(split: false);
		}
		else
		{
			UIInventoryHelper.TryMoveSlotInInventory(from, to);
		}
	}

	void IVendorDealHandler.HandleVendorDeal()
	{
		UpdatePlayerSide();
	}

	void IVendorDealHandler.HandleCancelVendorDeal()
	{
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

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM from, bool immediately)
	{
		UIInventoryHelper.TryMoveSlotInInventory(from, StashVM.FirstEmptySlot);
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		UpdatePlayerSide();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		StashVM.CollectionChanged();
	}
}
