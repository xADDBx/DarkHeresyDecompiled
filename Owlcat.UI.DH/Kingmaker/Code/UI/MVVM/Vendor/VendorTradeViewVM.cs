using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Vendor;

public class VendorTradeViewVM : ViewModel, INewSlotsHandler, ISubscriber, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IVendorTransferHandler, IVendorDealPriceChangeHandler, IRefreshVisibleCollectionHandler, IVendorTrashSellStateChangedHandler
{
	private readonly ReactiveProperty<string> m_VendorName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_VendorFactionName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Sprite> m_VendorSprite = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<Sprite> m_FactionSprite = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<FactionType?> m_VendorFaction = new ReactiveProperty<FactionType?>();

	private readonly ReactiveProperty<int> m_VendorFearReputationLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_VendorRespectReputationLevel = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsMaxLevel = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsPossibleDeal = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSellTrash = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_SellTrashButtonLayer = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_DealPrice = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_VendorExchangePrice = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PlayerExchangePrice = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_CanVendorExchangeReturn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanPlayerExchangeReturn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_VendorHasItemsToSell = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<VendorTransitionWindowVM> m_TransitionWindowVM = new ReactiveProperty<VendorTransitionWindowVM>();

	private readonly Action m_CloseAction;

	private readonly MechanicEntity VendorEntity;

	private readonly Dictionary<ItemEntity, int> m_PlayerExchangeItemsCached = new Dictionary<ItemEntity, int>();

	public readonly PartyVM PartyVM;

	public readonly SlotsGroupVM<ItemSlotVM> VendorSlotsGroup;

	public readonly ItemsFilterVM VendorItemsFilter;

	public readonly InventoryStashVM StashVM;

	public readonly SlotsGroupVM<ItemSlotVM> PlayerExchangePart;

	public readonly SlotsGroupVM<ItemSlotVM> VendorExchangePart;

	public bool HasFaction
	{
		get
		{
			if (m_VendorFaction.Value.HasValue)
			{
				return m_VendorFaction.Value.Value != FactionType.None;
			}
			return false;
		}
	}

	public long PlayerMoney => Game.Instance.Player.Money;

	private TradeLogic Vendor => VendorHelper.TradeLogic;

	public ReadOnlyReactiveProperty<string> VendorName => m_VendorName;

	public ReadOnlyReactiveProperty<string> VendorFactionName => m_VendorFactionName;

	public ReadOnlyReactiveProperty<Sprite> VendorSprite => m_VendorSprite;

	public ReadOnlyReactiveProperty<Sprite> FactionSprite => m_FactionSprite;

	public ReadOnlyReactiveProperty<FactionType?> VendorFaction => m_VendorFaction;

	public ReadOnlyReactiveProperty<int> VendorFearReputationLevel => m_VendorFearReputationLevel;

	public ReadOnlyReactiveProperty<int> VendorRespectReputationLevel => m_VendorRespectReputationLevel;

	public ReadOnlyReactiveProperty<bool> IsMaxLevel => m_IsMaxLevel;

	public ReadOnlyReactiveProperty<bool> IsPossibleDeal => m_IsPossibleDeal;

	public ReadOnlyReactiveProperty<bool> CanSellTrash => m_CanSellTrash;

	public ReadOnlyReactiveProperty<int> SellTrashButtonLayer => m_SellTrashButtonLayer;

	public ReadOnlyReactiveProperty<int> DealPrice => m_DealPrice;

	public ReadOnlyReactiveProperty<int> VendorExchangePrice => m_VendorExchangePrice;

	public ReadOnlyReactiveProperty<int> PlayerExchangePrice => m_PlayerExchangePrice;

	public ReadOnlyReactiveProperty<bool> VendorHasItemsToSell => m_VendorHasItemsToSell;

	public ReadOnlyReactiveProperty<VendorTransitionWindowVM> TransitionWindowVM => m_TransitionWindowVM;

	public bool HasDiscount => VendorHelper.TradeLogic.HasDiscount;

	public int DiscountValue => VendorHelper.TradeLogic.DiscountValue;

	public VendorTradeViewVM(MechanicEntity vendor, Action closeAction)
	{
		VendorEntity = vendor;
		m_CloseAction = closeAction;
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Vendor);
		});
		m_VendorFaction.Value = Game.Instance.TradeLogic.VendorFaction?.FactionType;
		m_VendorName.Value = (Game.Instance.TradeLogic.VendorName ?? string.Empty) ?? "";
		m_VendorSprite.Value = Game.Instance.TradeLogic.VendorIcon;
		m_FactionSprite.Value = Game.Instance.TradeLogic.VendorFaction?.Icon;
		m_VendorFactionName.Value = Game.Instance.TradeLogic.VendorFaction?.DisplayName.Text;
		var (value, value2) = (ReputationDescription)(ref VendorHelper.TradeLogic.Reputation);
		m_VendorFearReputationLevel.Value = value;
		m_VendorRespectReputationLevel.Value = value2;
		m_IsMaxLevel.Value = false;
		ItemsSortingContext sortingCtx = new ItemsSortingContext
		{
			GetComparisonFunc = GetVendorComparison
		};
		VendorSlotsGroup = new ItemSlotsGroupVM(Vendor.StoreItems, 1, 10, sortingCtx);
		VendorSlotsGroup.AddTo(this);
		VendorItemsFilter = new ItemsFilterVM(VendorSlotsGroup).AddTo(this);
		VendorItemsFilter.SetCurrentFilter(ItemsFilterType.NoFilter);
		ObservableSubscribeExtensions.Subscribe(VendorSlotsGroup.CollectionChangedCommand, delegate
		{
			m_VendorHasItemsToSell.Value = VendorSlotsGroup.VisibleCollection.Any((ItemSlotVM i) => i.HasItem);
		}).AddTo(this);
		ItemsSortingContext sortingCtx2 = new ItemsSortingContext
		{
			SorterType = Game.Instance.Player.UISettings.InventorySorter,
			FilterType = Game.Instance.Player.UISettings.InventoryFilter,
			RemoveEmptySlots = true
		};
		ItemSlotsGroupVM itemSlotsGroupVM = new ItemSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, 5, 100, sortingCtx2, Game.Instance.Player.UISettings.ShowUnavailableItems, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory);
		itemSlotsGroupVM.AddTo(this);
		StashVM = new InventoryStashVM(itemSlotsGroupVM).AddTo(this);
		ItemsSortingContext sortingCtx3 = new ItemsSortingContext
		{
			RemoveEmptySlots = true
		};
		PlayerExchangePart = new ItemSlotsGroupVM(Vendor.ItemsForSell, 3, 21, sortingCtx3).AddTo(this);
		m_PlayerExchangeItemsCached.Clear();
		ObservableSubscribeExtensions.Subscribe(PlayerExchangePart.CollectionChangedCommand, delegate
		{
			BlinkNewItems(PlayerExchangePart.VisibleCollection, in m_PlayerExchangeItemsCached);
		}).AddTo(this);
		VendorExchangePart = new ItemSlotsGroupVM(Vendor.ItemsForBuy, 3, 21, sortingCtx3).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdateHandler();
		}).AddTo(this);
		m_VendorHasItemsToSell.Value = Vendor.StoreItems.Any();
		PartyVM = new PartyVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		UpdateSellTrashButtonState();
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
		throw new NotImplementedException();
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (!(slot.Owner is BaseUnitEntity { IsPreviewUnit: not false }))
		{
			UpdatePlayerSide();
		}
	}

	private void UpdateVendorSide()
	{
		VendorSlotsGroup.UpdateVisibleCollection();
		VendorExchangePart.UpdateVisibleCollection();
	}

	private void UpdatePlayerSide()
	{
		StashVM.CollectionChanged();
		PlayerExchangePart.UpdateVisibleCollection();
	}

	private void BlinkNewItems(IReadOnlyList<ItemSlotVM> slotsCollection, in Dictionary<ItemEntity, int> cachedItems)
	{
		foreach (ItemSlotVM item in slotsCollection)
		{
			ItemEntity currentValue = item.Item.CurrentValue;
			if (currentValue != null && (!cachedItems.TryGetValue(currentValue, out var value) || value < currentValue.Count))
			{
				item.Blink();
			}
		}
		cachedItems.Clear();
		foreach (ItemSlotVM item2 in slotsCollection)
		{
			ItemEntity currentValue2 = item2.Item.CurrentValue;
			if (currentValue2 != null)
			{
				cachedItems.Add(currentValue2, currentValue2.Count);
			}
		}
	}

	private void OnUpdateHandler()
	{
		m_IsPossibleDeal.Value = Vendor.IsDealPossible;
		m_DealPrice.Value = Vendor.DealPrice;
		m_VendorExchangePrice.Value = Vendor.ItemsForBuyPrice;
		m_PlayerExchangePrice.Value = Vendor.ItemsForSellPrice;
		m_CanVendorExchangeReturn.Value = Vendor.ItemsForBuy.Any();
		m_CanPlayerExchangeReturn.Value = Vendor.ItemsForSell.Any();
	}

	public void BackToStash()
	{
	}

	public void MoveTrashForSale()
	{
		Vendor.MoveTrash();
	}

	public void ShowTrashInVendor(bool show)
	{
		Vendor.ShowSoldItemsToVendorFilter.Value = show;
	}

	public void HideUnavailable(bool hide)
	{
		Vendor.HideUnavailable.Value = hide;
	}

	private void UpdateSellTrashButtonState()
	{
		m_SellTrashButtonLayer.Value = ((Vendor.HasTrashForSale && !Vendor.HasTrashInPlayerStash) ? 1 : 0);
		m_CanSellTrash.Value = Vendor.HasTrashForSale || Vendor.HasTrashInPlayerStash;
	}

	public void Close()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode && !UtilityNet.IsControlMainCharacterWithWarning())
		{
			return;
		}
		if (Vendor.IsChanged)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.Vendor.BeforeClose, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					Game.Instance.GameCommandQueue.EndTrading();
					m_CloseAction?.Invoke();
				}
			});
		}
		else
		{
			Game.Instance.GameCommandQueue.EndTrading();
			m_CloseAction?.Invoke();
		}
	}

	public void Deal()
	{
		UISound dealSound = GetDealSound();
		Vendor.Deal(VendorEntity);
		UpdatePlayerSide();
		UpdateVendorSide();
		dealSound.Play();
	}

	private UISound GetDealSound()
	{
		if (Vendor.ItemsForBuyPrice == 0 || 1f * (float)Vendor.ItemsForSellPrice / (float)Vendor.ItemsForBuyPrice > UIConfig.Instance.VendorConfig.DealPriceRatio)
		{
			return FullScreenSounds.Instance.Vendor.DealButtonSell;
		}
		if (Vendor.ItemsForSellPrice == 0 || 1f * (float)Vendor.ItemsForBuyPrice / (float)Vendor.ItemsForSellPrice > UIConfig.Instance.VendorConfig.DealPriceRatio)
		{
			return FullScreenSounds.Instance.Vendor.DealButtonBuy;
		}
		return FullScreenSounds.Instance.Vendor.DealButtonNormal;
	}

	public string DealInactiveWarning()
	{
		if (!Vendor.IsDealPossible)
		{
			if (!Vendor.IsEnoughMoneyToDeal)
			{
				return UIStrings.Instance.Vendor.NotEnoughMoney.Text;
			}
			return UIStrings.Instance.Vendor.NoItemsToDeal.Text;
		}
		return string.Empty;
	}

	void IVendorDealPriceChangeHandler.HandlePlayerPriceChanged()
	{
		UpdatePlayerSide();
	}

	void IVendorDealPriceChangeHandler.HandleVendorPriceChanged()
	{
		UpdateVendorSide();
	}

	void IVendorDealPriceChangeHandler.HandleDealPriceChanged()
	{
		UpdatePlayerSide();
		UpdateVendorSide();
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
		{
			h.HandleOpen(CounterWindowType.Split, slot.Item.CurrentValue, slot.SplitItem);
		});
	}

	void IRefreshVisibleCollectionHandler.Refresh()
	{
		UpdatePlayerSide();
		UpdateVendorSide();
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if ((to.Group != VendorExchangePart || to.Group != from.Group) && (to.Group != PlayerExchangePart || to.Group != from.Group) && !MoveItemFromVendorToBuy(from, to) && !MoveItemFromBuyToVendor(from, to) && !MoveItemFromStashToSell(from, to))
		{
			MoveItemFromSellToStash(from, to);
		}
	}

	void IVendorTrashSellStateChangedHandler.HandleTrashSellStateChanged()
	{
		UpdateSellTrashButtonState();
	}

	private bool MoveItemFromVendorToBuy(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.IsLockedByRep)
		{
			return false;
		}
		if (from.IsInVendor && to.Group == VendorExchangePart && from.HasItem)
		{
			VendorHelper.TryMove(from.ItemEntity, from.ParentCollection, from.ItemEntity.Count, split: true);
			return true;
		}
		return false;
	}

	private bool MoveItemFromBuyToVendor(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.ParentCollection == Vendor.ItemsForBuy && to.Group == VendorSlotsGroup && from.HasItem)
		{
			VendorHelper.TryMove(from.ItemEntity, from.ParentCollection, from.ItemEntity.Count, split: false);
			return true;
		}
		return false;
	}

	private bool MoveItemFromStashToSell(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.ParentCollection == Game.Instance.PartySharedInventory.Collection && to.Group == PlayerExchangePart && from.HasItem)
		{
			VendorHelper.TryMove(from.ItemEntity, from.ParentCollection, from.ItemEntity.Count, split: false);
			return true;
		}
		return false;
	}

	private bool MoveItemFromSellToStash(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.ParentCollection == Vendor.ItemsForSell && to.Group == StashVM.ItemSlotsGroup && from.HasItem)
		{
			VendorHelper.TryMove(from.ItemEntity, from.ParentCollection, from.ItemEntity.Count, split: false);
			return true;
		}
		return false;
	}

	public void HandleTransitionWindow(ItemEntity itemEntity = null)
	{
		CloseTransitionWindow();
		m_TransitionWindowVM.Value = new VendorTransitionWindowVM(Vendor, itemEntity, CloseTransitionWindow);
	}

	private void CloseTransitionWindow()
	{
		TransitionWindowVM.CurrentValue?.Dispose();
		m_TransitionWindowVM.Value = null;
	}

	private Comparison<ItemEntity> GetVendorComparison(ItemsSorterType sorter, ItemsFilterType filter)
	{
		if (sorter != 0)
		{
			return ItemsFilter.GetItemsDefaultComparison(sorter, filter);
		}
		return (ItemEntity a, ItemEntity b) => CompareByVendorRestrictions(a, b, VendorFaction.CurrentValue.Value);
	}

	private static int CompareByVendorRestrictions(ItemEntity a, ItemEntity b, FactionType vendorFaction)
	{
		VendorTableSlot vendorSlot = a.GetVendorSlot();
		VendorTableSlot vendorSlot2 = b.GetVendorSlot();
		if (vendorSlot == null && vendorSlot2 == null)
		{
			return 0;
		}
		if (vendorSlot != null && vendorSlot2 == null)
		{
			return -1;
		}
		if (vendorSlot == null)
		{
			return 1;
		}
		ReputationRestriction reputationRestriction = vendorSlot.ReputationRestriction;
		ReputationRestriction reputationRestriction2 = vendorSlot2.ReputationRestriction;
		bool flag = reputationRestriction.IsPassed(vendorFaction);
		bool flag2 = reputationRestriction2.IsPassed(vendorFaction);
		if (flag && !flag2)
		{
			return -1;
		}
		if (!flag && flag2)
		{
			return 1;
		}
		int num = reputationRestriction2.Type.CompareTo(reputationRestriction.Type);
		int num2 = CompareByPriceAscending(a, b);
		if (!flag)
		{
			if (num != 0)
			{
				return num;
			}
			int num3 = reputationRestriction.Value.CompareTo(reputationRestriction2.Value);
			if (num3 != 0)
			{
				return num3;
			}
			return num2;
		}
		if (num2 == 0)
		{
			return num;
		}
		return num2;
		int CompareByPriceAscending(ItemEntity lhs, ItemEntity rhs)
		{
			int itemCost = VendorHelper.TradeLogic.GetItemCost(a);
			int itemCost2 = VendorHelper.TradeLogic.GetItemCost(b);
			return itemCost.CompareTo(itemCost2);
		}
	}
}
