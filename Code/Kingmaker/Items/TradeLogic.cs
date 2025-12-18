using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GameConst;
using R3;
using UnityEngine;

namespace Kingmaker.Items;

public class TradeLogic : IItemsCollectionHandler, ISubscriber
{
	private sealed class UIDealNotification
	{
		private readonly StringBuilder _builder = new StringBuilder();

		private int _count;

		private static int MaxCount => UIConsts.ItemsToShowLootNotificationWindow;

		public void Add(ItemEntity item)
		{
			_count++;
			if (_count <= MaxCount)
			{
				_builder.AppendLine("<b>" + item.Name + "</b>");
			}
		}

		public void Show()
		{
			if (_count > MaxCount)
			{
				_builder.AppendLine(string.Format(UIStrings.Instance.LootWindow.NotificationMessageForGainLootItem, _count - MaxCount));
			}
			if (_builder.ToString().IsNullOrEmpty())
			{
				return;
			}
			using (GameLogContext.Scope)
			{
				GameLogContext.Text = _builder.ToString();
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(GameLogStrings.Instance.ItemGained.Message.Text ?? "", addToLog: false);
				});
			}
		}
	}

	private MechanicEntity m_VendorEntity;

	private readonly List<ItemEntity> m_cachedItems = new List<ItemEntity>();

	private readonly HashSet<ItemEntity> m_trashInPlayerStash = new HashSet<ItemEntity>();

	private readonly HashSet<ItemEntity> m_trashForSale = new HashSet<ItemEntity>();

	public readonly ReactiveProperty<bool> ShowSoldItemsToVendorFilter = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HideUnavailable = new ReactiveProperty<bool>();

	public bool HasTrashInPlayerStash => m_trashInPlayerStash.Count > 0;

	public bool HasTrashForSale => m_trashForSale.Count > 0;

	public int DealPrice { get; private set; }

	public int ItemsForBuyPrice { get; private set; }

	public int ItemsForSellPrice { get; private set; }

	private static VendorsManager VendorsManager => Game.Instance.VendorsManager;

	public ItemsCollection ItemsForBuy => VendorsManager.ItemsForBuy;

	public ItemsCollection ItemsForSell => VendorsManager.ItemsForSell;

	public bool IsTrading => m_VendorEntity != null;

	public MechanicEntity VendorEntity => m_VendorEntity;

	public BlueprintVendorFaction VendorFaction => VendorInventory?.Faction;

	public FactionType VendorFactionType => VendorFaction.FactionType;

	public ReputationDescription Reputation => ReputationHelper.GetReputation(VendorFactionType);

	private BaseUnitEntity VendorUnit => m_VendorEntity as BaseUnitEntity;

	public string VendorName => VendorUnit?.Blueprint?.CharacterName ?? VendorEntity.Blueprint.Name;

	public PortraitData VendorPortrait => VendorUnit?.Portrait;

	public Sprite VendorIcon => m_VendorEntity?.Blueprint?.Icon;

	public PartVendor VendorInventory => m_VendorEntity?.GetOptional<PartVendor>();

	public bool NeedHideCostAndReputation
	{
		get
		{
			if (m_VendorEntity != null)
			{
				return m_VendorEntity.Blueprint.GetComponent<AddSharedVendor>().NeedHideCostAndReputation;
			}
			return false;
		}
	}

	public ItemsCollection StoreItems => VendorInventory?.Collection;

	public ItemsCollection PlayerStash => Game.Instance.PartySharedInventory.Collection;

	public bool IsDealPossible
	{
		get
		{
			if (ItemsForBuy.Items.Count > 0 || ItemsForSell.Items.Count > 0)
			{
				return Game.Instance.Player.Money + DealPrice >= 0;
			}
			return false;
		}
	}

	public bool IsChanged
	{
		get
		{
			if (IsTrading)
			{
				if (ItemsForBuy.Items.Count <= 0)
				{
					return DealPrice != 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasDiscount => VendorsManager.GetDiscount(VendorFactionType) > 0;

	public int DiscountValue => VendorsManager.GetDiscount(VendorFactionType);

	public bool BeginTrading(MechanicEntity vendor)
	{
		if (vendor == null)
		{
			PFLog.Default.Error("BeginTrading: vendor is null");
			return false;
		}
		if (m_VendorEntity != null)
		{
			if (m_VendorEntity == vendor)
			{
				PFLog.Default.Warning($"Previous trading with the same vendor {m_VendorEntity} is not finished");
				return false;
			}
			PFLog.Default.Error($"Previous trading with {m_VendorEntity} is not finished");
			EndTrading();
		}
		m_VendorEntity = vendor;
		if (VendorInventory == null)
		{
			PFLog.Default.Error($"Trading with {vendor} can't start: no vendor table");
			m_VendorEntity = null;
			return false;
		}
		if (VendorFaction == null)
		{
			PFLog.Default.Error($"Trading with {vendor} can't start: no VendorFaction Part");
			m_VendorEntity = null;
			return false;
		}
		EventBus.RaiseEvent((IMechanicEntity)vendor, (Action<ITradeStateChanged>)delegate(ITradeStateChanged h)
		{
			h.HandleVendorAboutToTrading();
		}, isCheckRuntime: true);
		foreach (ItemEntity item in VendorInventory)
		{
			item.Identify();
		}
		bool autoIdentifyPlayersInventory = VendorInventory.AutoIdentifyPlayersInventory;
		m_trashInPlayerStash.Clear();
		m_trashForSale.Clear();
		m_cachedItems.Clear();
		foreach (ItemEntity item2 in PlayerStash)
		{
			if (autoIdentifyPlayersInventory)
			{
				item2.Identify();
			}
			if (VendorsManager.IsTrash(item2) && item2.Wielder == null)
			{
				m_trashInPlayerStash.Add(item2);
			}
		}
		VendorsManager.CleanupTable(VendorInventory.VendorTable);
		ResetInfo();
		EventBus.RaiseEvent((IMechanicEntity)vendor, (Action<ITradeStateChanged>)delegate(ITradeStateChanged h)
		{
			h.HandleBeginTrading();
		}, isCheckRuntime: true);
		CacheDetectedVendor(vendor);
		EventBus.Subscribe(this);
		return true;
	}

	public void MoveTrash()
	{
		if (HasTrashInPlayerStash)
		{
			m_cachedItems.Clear();
			m_cachedItems.AddRange(m_trashInPlayerStash);
			foreach (ItemEntity cachedItem in m_cachedItems)
			{
				if (cachedItem.Wielder == null && cachedItem.Count > 0)
				{
					m_trashForSale.Add(cachedItem);
					AddForSellInternal(cachedItem, cachedItem.Count);
				}
			}
			m_trashInPlayerStash.Clear();
			UpdatePlayerDeal();
		}
		else
		{
			if (!HasTrashForSale)
			{
				return;
			}
			m_cachedItems.Clear();
			m_cachedItems.AddRange(m_trashForSale);
			foreach (ItemEntity cachedItem2 in m_cachedItems)
			{
				if (cachedItem2.Count > 0 && cachedItem2.Wielder == null)
				{
					m_trashInPlayerStash.Add(cachedItem2);
					RemoveFromSaleInternal(cachedItem2, cachedItem2.Count);
				}
			}
			m_trashForSale.Clear();
			UpdatePlayerDeal();
		}
	}

	private static void CacheDetectedVendor([NotNull] MechanicEntity vendor)
	{
		if (!VendorsManager.DetectedVendors.Any((DetectedVendorData x) => x.EntityBlueprint == vendor.Blueprint))
		{
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea == null)
			{
				PFLog.Default.Error("Can not detect vendor " + vendor.UniqueId + " cause no loaded area");
				return;
			}
			BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
			int chapter = Game.Instance.Player.Chapter;
			PartLastDetectedLocation orCreate = vendor.GetOrCreate<PartLastDetectedLocation>();
			orCreate.DetectLocation(currentlyLoadedArea, currentlyLoadedAreaPart, chapter);
			VendorsManager.AddDetectedVendor(vendor, currentlyLoadedArea, currentlyLoadedAreaPart, chapter);
			PFLog.Default.Log("Detect vendor {0}, {1}, {2}, {3}, {4}", vendor.UniqueId, vendor.GetOptional<PartVendor>()?.Faction, orCreate.Area, orCreate.AreaPart, chapter);
		}
	}

	public void EndTrading()
	{
		ReturnItems();
		UpdateDeal();
		m_VendorEntity = null;
		EventBus.RaiseEvent(delegate(ITradeStateChanged h)
		{
			h.HandleEndTrading();
		});
		EventBus.Unsubscribe(this);
	}

	private void ResetInfo()
	{
		DealPrice = 0;
		ItemsForBuyPrice = 0;
		ItemsForSellPrice = 0;
	}

	private void ReturnItems()
	{
		if (m_VendorEntity != null)
		{
			ItemsForBuy.Items.ToArray().ForEach(delegate(ItemEntity i)
			{
				ItemsForBuy.Transfer(i, StoreItems);
			});
			ItemsForSell.Items.ToArray().ForEach(delegate(ItemEntity i)
			{
				ItemsForSell.Transfer(i, PlayerStash);
			});
		}
		else
		{
			ItemsForBuy.RemoveAll();
		}
	}

	public int GetBaseItemCost([CanBeNull] ItemEntity itemEntity)
	{
		int num = itemEntity.GetVendorSlot()?.OverrideCost ?? 0;
		if (num <= 0)
		{
			return itemEntity?.Blueprint.Cost ?? 0;
		}
		return num;
	}

	public int GetItemCost([CanBeNull] ItemEntity itemEntity)
	{
		if (itemEntity == null)
		{
			return 0;
		}
		int baseItemCost = GetBaseItemCost(itemEntity);
		if (itemEntity.Blueprint.Rarity == BlueprintItem.ItemRarity.Trash)
		{
			return baseItemCost;
		}
		if (itemEntity.IsFromVendorSlot())
		{
			int discount = VendorsManager.GetDiscount(VendorFaction.FactionType);
			float num = Math.Clamp(1f - (float)discount / 100f, 0f, 1f);
			return Mathf.RoundToInt((float)baseItemCost * num);
		}
		float num2 = Math.Clamp((float)ConfigRoot.Instance.SystemMechanics.VendorRoot.SellToVendorCostFactor / 100f, 0f, 1f);
		return Mathf.RoundToInt((float)baseItemCost * num2);
	}

	public bool IsItemSoldToVendor([CanBeNull] ItemEntity itemEntity)
	{
		return itemEntity?.IsSoldByPlayer() ?? false;
	}

	public bool IsReputationRestrictionPassed([CanBeNull] ItemEntity itemEntity)
	{
		return itemEntity?.GetReputationRestriction()?.IsPassed(VendorFactionType) ?? true;
	}

	public bool IsCostRestrictionPassed([CanBeNull] ItemEntity itemEntity)
	{
		return GetItemCost(itemEntity) <= Game.Instance.Player.Money;
	}

	public bool IsRestrictionPassed([CanBeNull] ItemEntity itemEntity)
	{
		if (itemEntity != null && itemEntity.Count > 0 && IsReputationRestrictionPassed(itemEntity))
		{
			return itemEntity.GetVendorSlot()?.Restriction.Check() ?? true;
		}
		return false;
	}

	public void AddForSell(ItemEntity item, int count)
	{
		if (item.Collection != PlayerStash)
		{
			PFLog.Default.Error("Item is not in 'player stash' collection");
			return;
		}
		AddForSellInternal(item, count);
		UpdatePlayerDeal();
	}

	public void RemoveFromSell(ItemEntity item, int count)
	{
		if (item.Collection != ItemsForSell)
		{
			PFLog.Default.Error("Item is not in 'sell' collection");
			return;
		}
		RemoveFromSaleInternal(item, count);
		UpdatePlayerDeal();
	}

	public ItemEntity AddForBuy(ItemEntity item, int count)
	{
		if (item.Collection != StoreItems)
		{
			PFLog.Default.Error("Item is not in 'store' collection");
			return null;
		}
		VendorTableSlot vendorSlot = item.GetVendorSlot();
		if (vendorSlot != null)
		{
			ReputationRestriction reputationRestriction = vendorSlot.ReputationRestriction;
			if (reputationRestriction != null && !reputationRestriction.IsPassed(VendorFactionType))
			{
				int num = Game.Instance.Reputation.Get(VendorFactionType, reputationRestriction.Type);
				PFLog.Default.Log("Item {0} locked by reputation {1} ({2}/{3})", item.Name, reputationRestriction.Type, num, reputationRestriction.Value);
				return null;
			}
		}
		count = ((count < 0) ? item.Count : count);
		ItemEntity result = item.Collection.Transfer(item, count, ItemsForBuy);
		UpdateVendorDeal();
		return result;
	}

	public ItemEntity RemoveFromBuy(ItemEntity item, int count)
	{
		if (item.Collection != ItemsForBuy)
		{
			PFLog.Default.Error("Item is not in 'buy' collection");
			return null;
		}
		count = ((count < 0) ? item.Count : count);
		ItemEntity result = item.Collection.Transfer(item, count, StoreItems);
		UpdateVendorDeal();
		return result;
	}

	public void CancelDeal()
	{
		List<ItemEntity> list = ItemsForBuy.Where((ItemEntity item) => item != null).ToList();
		if (list.Count > 0)
		{
			list.ForEach(delegate(ItemEntity item)
			{
				RemoveFromBuy(item, -1).TryMergeInCollection();
			});
			EventBus.RaiseEvent(delegate(IVendorDealHandler h)
			{
				h.HandleCancelVendorDeal();
			});
		}
	}

	public void MakeDealWithCurrentVendor()
	{
		Deal(m_VendorEntity);
	}

	public void Deal(MechanicEntity vendor)
	{
		if (!IsDealPossible)
		{
			PFLog.Default.Error("Trade deal is impossible");
			return;
		}
		Metrics.VendorDeal.Bought(ItemsForBuy.Select((ItemEntity i) => i.Blueprint.AssetGuid)).BoughtAmounts(ItemsForBuy.Select((ItemEntity i) => i.Count.ToString())).Sold(ItemsForSell.Select((ItemEntity i) => i.Blueprint.AssetGuid))
			.SoldAmounts(ItemsForSell.Select((ItemEntity i) => i.Count.ToString()))
			.Money(Game.Instance.Player.Money.ToString())
			.Price(DealPrice.ToString())
			.Send();
		Game.Instance.Statistic.HandleVendorDeal(vendor, ItemsForBuy);
		ItemEntity dealItem = null;
		UIDealNotification uIDealNotification = new UIDealNotification();
		ItemEntity[] array = ItemsForBuy.ToArray();
		foreach (ItemEntity itemEntity in array)
		{
			itemEntity.SellTime = Game.Instance.Player.GameTime;
			itemEntity.SetVendorIfNull(VendorEntity);
			itemEntity.RemoveVendorSlotData();
			ItemsForBuy.Transfer(itemEntity, Game.Instance.PartySharedInventory.Collection);
			dealItem = itemEntity;
			uIDealNotification.Add(itemEntity);
		}
		array = ItemsForSell.ToArray();
		foreach (ItemEntity itemEntity2 in array)
		{
			if (itemEntity2.Blueprint.Rarity == BlueprintItem.ItemRarity.Trash)
			{
				ItemsForSell.Remove(itemEntity2);
				continue;
			}
			ItemEntity itemEntity3 = ItemsForSell.Transfer(itemEntity2, StoreItems);
			itemEntity3.MarkAsSoldByPlayer();
			itemEntity3.SellTime = Game.Instance.Player.GameTime;
			itemEntity3.SetVendorIfNull(vendor);
		}
		m_trashForSale.Clear();
		Game.Instance.Player.GainMoney(DealPrice);
		ReturnItems();
		UpdateDeal();
		EventBus.RaiseEvent(delegate(IVendorDealHandler h)
		{
			h.HandleVendorDeal();
		});
		EventBus.RaiseEvent(delegate(IVendorBuyHandler h)
		{
			h.HandleBuyItem(dealItem);
		});
		uIDealNotification.Show();
	}

	void IItemsCollectionHandler.HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && VendorsManager.IsTrash(item) && item.Wielder == null)
		{
			if (collection == PlayerStash && item.Count > 0)
			{
				m_trashInPlayerStash.Add(item);
			}
			if (collection == ItemsForSell && item.Count > 0)
			{
				m_trashForSale.Add(item);
			}
			EventBus.RaiseEvent(delegate(IVendorTrashSellStateChangedHandler h)
			{
				h.HandleTrashSellStateChanged();
			});
		}
	}

	void IItemsCollectionHandler.HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && VendorsManager.IsTrash(item))
		{
			if (collection == PlayerStash && item.Collection != PlayerStash)
			{
				m_trashInPlayerStash.Remove(item);
			}
			if (collection == ItemsForSell && item.Collection != ItemsForSell)
			{
				m_trashForSale.Remove(item);
			}
			EventBus.RaiseEvent(delegate(IVendorTrashSellStateChangedHandler h)
			{
				h.HandleTrashSellStateChanged();
			});
		}
	}

	private void AddForSellInternal(ItemEntity item, int count)
	{
		count = ((count < 0) ? item.Count : count);
		item.Collection.Transfer(item, count, ItemsForSell);
	}

	private void RemoveFromSaleInternal(ItemEntity item, int count)
	{
		count = ((count < 0) ? item.Count : count);
		item.Collection.Transfer(item, count, PlayerStash);
	}

	private int CalculatePriceSum(IEnumerable<ItemEntity> items)
	{
		int num = 0;
		foreach (ItemEntity item in items)
		{
			num += GetItemCost(item) * item.Count;
		}
		return num;
	}

	private void UpdateVendorDeal()
	{
		ItemsForBuyPrice = CalculatePriceSum(ItemsForBuy);
		DealPrice = ItemsForSellPrice - ItemsForBuyPrice;
		EventBus.RaiseEvent(delegate(IVendorDealPriceChangeHandler h)
		{
			h.HandleVendorPriceChanged();
		});
	}

	private void UpdatePlayerDeal()
	{
		ItemsForSellPrice = CalculatePriceSum(ItemsForSell);
		DealPrice = ItemsForSellPrice - ItemsForBuyPrice;
		EventBus.RaiseEvent(delegate(IVendorDealPriceChangeHandler h)
		{
			h.HandlePlayerPriceChanged();
		});
	}

	private void UpdateDeal()
	{
		ItemsForBuyPrice = CalculatePriceSum(ItemsForBuy);
		ItemsForSellPrice = CalculatePriceSum(ItemsForSell);
		DealPrice = ItemsForSellPrice - ItemsForBuyPrice;
		EventBus.RaiseEvent(delegate(IVendorDealPriceChangeHandler h)
		{
			h.HandleDealPriceChanged();
		});
	}
}
