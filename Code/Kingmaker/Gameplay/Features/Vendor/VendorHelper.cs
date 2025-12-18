using System;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Gameplay.Features.Vendor;

public static class VendorHelper
{
	public static TradeLogic TradeLogic => Game.Instance.TradeLogic;

	public static void TryMove(ItemEntity itemEntity, ItemsCollection collection, bool split)
	{
		if (!TradeLogic.IsTrading)
		{
			return;
		}
		if (collection == TradeLogic.StoreItems)
		{
			if (itemEntity.Count <= 1)
			{
				Game.Instance.GameCommandQueue.AddForBuyVendor(itemEntity, itemEntity.Count);
				return;
			}
			EventBus.RaiseEvent(delegate(IVendorTransferHandler h)
			{
				h.HandleTransitionWindow(itemEntity);
			});
		}
		else if (collection == TradeLogic.PlayerStash)
		{
			if (itemEntity.IsNotable)
			{
				return;
			}
			if (itemEntity.Count <= 1)
			{
				Game.Instance.GameCommandQueue.AddForSellVendor(itemEntity, itemEntity.Count);
				return;
			}
			EventBus.RaiseEvent(delegate(IVendorTransferHandler h)
			{
				h.HandleTransitionWindow(itemEntity);
			});
		}
		else if (collection == TradeLogic.ItemsForBuy)
		{
			Game.Instance.GameCommandQueue.RemoveFromBuyVendor(itemEntity, itemEntity.Count);
		}
		else if (collection == TradeLogic.ItemsForSell)
		{
			Game.Instance.GameCommandQueue.RemoveFromSellVendor(itemEntity, itemEntity.Count);
		}
	}

	public static void TryMoveSplit(ItemEntity itemEntity, bool split, Action<int> command)
	{
		if (TradeLogic.VendorInventory.IsLockedByReputation(itemEntity))
		{
			PFLog.UI.Log("Item {0} locked by reputation {1}/{2}", itemEntity.Name, TradeLogic.VendorInventory.GetCurrentFactionReputationPoints(), TradeLogic.VendorInventory.GetReputationToUnlock(itemEntity));
		}
		else if (split && itemEntity.Count > 1)
		{
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Move, itemEntity, command.Invoke);
			});
		}
		else
		{
			command(itemEntity.Count);
		}
	}

	[CanBeNull]
	public static PartItemInVendor GetVendorData([CanBeNull] this ItemEntity item)
	{
		return item?.GetOptional<PartItemInVendor>();
	}

	[NotNull]
	public static PartItemInVendor GetOrCreateVendorData([NotNull] this ItemEntity item)
	{
		return item.GetOrCreate<PartItemInVendor>();
	}

	public static bool IsFromSameVendorSlot(this ItemEntity item, ItemEntity other)
	{
		PartItemInVendor vendorData = item.GetVendorData();
		if (vendorData != null)
		{
			PartItemInVendor vendorData2 = other.GetVendorData();
			if (vendorData2 != null)
			{
				if (vendorData.VendorTable == vendorData2.VendorTable)
				{
					return vendorData.IdInVendorTable == vendorData2.IdInVendorTable;
				}
				return false;
			}
		}
		return false;
	}

	public static void CopyVendorDataFrom(this ItemEntity item, ItemEntity other)
	{
		PartItemInVendor vendorData = other.GetVendorData();
		if (vendorData != null)
		{
			item.GetOrCreateVendorData().CopyFrom(vendorData);
		}
	}

	[CanBeNull]
	public static VendorTableSlot FindVendorSlot(this BlueprintSharedVendorTable table, ItemEntity item)
	{
		PartItemInVendor slotData = item.GetVendorData();
		if (slotData == null)
		{
			return null;
		}
		if (slotData.VendorTable != table)
		{
			return null;
		}
		return table?.Slots.FirstItem((VendorTableSlot i) => i.Id == slotData.IdInVendorTable);
	}

	public static bool IsFromVendorSlot(this ItemEntity item)
	{
		PartItemInVendor vendorData = item.GetVendorData();
		if (vendorData != null)
		{
			return vendorData.VendorTable != null;
		}
		return false;
	}

	public static bool IsFromVendorSlot(this ItemEntity item, BlueprintSharedVendorTable table, VendorTableSlot slot)
	{
		PartItemInVendor vendorData = item.GetVendorData();
		if (vendorData != null && vendorData.VendorTable == table)
		{
			return vendorData.IdInVendorTable == slot.Id;
		}
		return false;
	}

	public static void SetVendorData(this ItemEntity item, BlueprintSharedVendorTable table, VendorTableSlot slot)
	{
		PartItemInVendor orCreateVendorData = item.GetOrCreateVendorData();
		orCreateVendorData.VendorTable = table;
		orCreateVendorData.IdInVendorTable = slot.Id;
	}

	[CanBeNull]
	public static VendorTableSlot GetVendorSlot(this ItemEntity item)
	{
		PartItemInVendor slotData = item.GetVendorData();
		if (slotData == null)
		{
			return null;
		}
		return slotData.VendorTable?.Slots.FirstItem((VendorTableSlot i) => i.Id == slotData.IdInVendorTable);
	}

	public static void RemoveVendorSlotData(this ItemEntity item)
	{
		item.Remove<PartItemInVendor>();
	}

	public static bool IsSoldByPlayer(this ItemEntity item)
	{
		return item.GetVendorData()?.SoldByPlayer ?? false;
	}

	public static void MarkAsSoldByPlayer(this ItemEntity item)
	{
		PartItemInVendor orCreateVendorData = item.GetOrCreateVendorData();
		orCreateVendorData.VendorTable = null;
		orCreateVendorData.IdInVendorTable = 0;
		orCreateVendorData.SoldByPlayer = true;
	}

	public static bool IsTrash(this ItemEntity item)
	{
		return item.Blueprint.IsTrash();
	}

	public static bool IsTrash(this BlueprintItem item)
	{
		return Game.Instance.Player.VendorsManager.IsTrash(item);
	}

	public static bool IsMarkedAsTrash(this ItemEntity item)
	{
		return item.Blueprint.IsMarkedAsTrash();
	}

	public static bool IsMarkedAsTrash(this BlueprintItem item)
	{
		return Game.Instance.Player.VendorsManager.IsMarkedAsTrash(item);
	}

	[Cheat(Name = "add_money", Description = "Add money to player (default = 1000)")]
	public static void AddMoney(int amount = 1000)
	{
		Game.Instance.Player.GainMoney(amount);
	}
}
