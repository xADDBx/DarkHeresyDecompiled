using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public static class UIInventoryHelper
{
	public enum EndDragAction
	{
		Put,
		Swap,
		Split,
		HalfSplit,
		Abort,
		Merge
	}

	private static ItemSlot s_ItemSlot;

	public static ItemSlotRef ToSlotRef(this ItemsCollection itemsCollection, int index)
	{
		return new ItemSlotRef(EquipSlotType.PrimaryHand, -1, index, itemsCollection.FirstOrDefault((ItemEntity x) => x.InventorySlotIndex == index), itemsCollection);
	}

	public static ItemSlotRef ToSlotRef(this ItemSlotVM slotVM)
	{
		if (slotVM is EquipSlotVM equipSlotVM)
		{
			return new ItemSlotRef(equipSlotVM.SlotType, equipSlotVM.SetIndex, -1, equipSlotVM.ItemEntity, slotVM.ItemEntity?.Collection ?? equipSlotVM.ItemSlot?.MaybeOwnerInventory?.Collection);
		}
		ItemsCollection collection = slotVM.ItemEntity?.Collection ?? slotVM.Group?.MechanicCollection;
		return new ItemSlotRef(EquipSlotType.PrimaryHand, -1, slotVM.Index, slotVM.ItemEntity, collection);
	}

	public static bool TryUnequip(EquipSlotVM slot)
	{
		ItemSlot itemSlot = slot.ItemSlot;
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
			return false;
		}
		if (RootUIContext.Instance.IsChargenShown)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInLevlUpIsImpossible, null, addToLog: false);
			});
			return false;
		}
		if (itemSlot == null || !itemSlot.HasItem || (itemSlot.HasItem && !itemSlot.CanRemoveItem()))
		{
			return false;
		}
		if (!(itemSlot.Owner is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (!baseUnitEntity.CanBeControlled())
		{
			return false;
		}
		Game.Instance.GameCommandQueue.UnequipItem(baseUnitEntity, slot.ToSlotRef(), null);
		UISounds.Instance.PlayItemSound(SlotAction.Take, slot.ItemEntity, equipSound: true);
		return true;
	}

	public static bool TryMoveToInventory(ItemSlotVM slot)
	{
		ItemEntity currentValue = slot.Item.CurrentValue;
		if (currentValue == null)
		{
			return false;
		}
		Game.Instance.GameCommandQueue.TransferItemsToInventory(new List<EntityRef<ItemEntity>> { currentValue });
		return true;
	}

	public static void TryEquip(ItemSlotVM slot, BaseUnitEntity unit)
	{
		if (unit == null || slot == null || !slot.HasItem || (unit is UnitEntity && !unit.CanBeControlled()))
		{
			return;
		}
		ItemEntity item = slot.ItemEntity;
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
		}
		else if (RootUIContext.Instance.IsChargenShown)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInLevlUpIsImpossible, null, addToLog: false);
			});
		}
		else if (!unit.Body.AllSlots.Any((ItemSlot sl) => sl.CanInsertItem(item)))
		{
			if (RootUIContext.Instance.IsInventoryShow)
			{
				EventBus.RaiseEvent(delegate(IInventoryNotificationHandler h)
				{
					h.HandleWarning(UIStrings.Instance.InventoryScreen.CantInsertThisWeapon.Text);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.InventoryScreen.CantInsertThisWeapon.Text, addToLog: false, WarningNotificationFormat.Warning);
				});
			}
		}
		else
		{
			Game.Instance.GameCommandQueue.EquipItem(item, unit, null);
			bool isNotable = item.Blueprint.IsNotable;
			if (UIUtilityItem.GetEquipPosibility(item)[0] || isNotable || item is ItemEntitySimple)
			{
				UISounds.Instance.PlayItemSound(SlotAction.Put, item, equipSound: true);
			}
			else
			{
				CombatSounds.Instance.Combat.CombatGridCantPerformActionClick.Play();
			}
		}
	}

	public static void TryDrop(ItemSlotVM slot)
	{
		if (slot != null && slot.HasItem)
		{
			ItemEntity currentValue = slot.Item.CurrentValue;
			if (currentValue != null)
			{
				TryDrop(currentValue);
			}
		}
	}

	public static void TryDrop(ItemEntity item)
	{
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
		}
		else if (RootUIContext.Instance.IsChargenShown)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInLevlUpIsImpossible, null, addToLog: false);
			});
		}
		else if (UtilityGame.IsGlobalMap())
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.CommonTexts.DropItemFromGlobalMap, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					_ = s_ItemSlot;
				}
			});
		}
		else
		{
			DropItemMechanic(item);
		}
	}

	private static void DropItemMechanic(ItemEntity item, bool showCounterWindow = false)
	{
		if (showCounterWindow && item.IsStackable && item.Count > 1)
		{
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Drop, item, delegate(int count)
				{
					Game.Instance.GameCommandQueue.DropItem(item, split: true, count);
				});
			});
		}
		else
		{
			Game.Instance.GameCommandQueue.DropItem(item, split: false, 0);
		}
	}

	public static void TrySplitSlot(ItemSlotVM slot, bool isLoot)
	{
		if (!slot.HasItem || !slot.ItemEntity.IsStackable || slot.ItemEntity.Count <= 1)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
		{
			h.HandleOpen(CounterWindowType.Split, slot.Item.CurrentValue, delegate(int count)
			{
				Game.Instance.GameCommandQueue.SplitSlot(slot.ToSlotRef(), null, isLoot, count);
			});
		});
	}

	public static void TryMoveSlotInInventory(ItemSlotVM from, ItemSlotVM to)
	{
		ReadOnlyReactiveProperty<ItemEntity> item = from.Item;
		bool fromShip = item != null && item.CurrentValue.Origin == ItemsItemOrigin.ShipComponents;
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IInventoryNotificationHandler h)
			{
				h.HandleWarning(LocalizedTexts.Instance.WarningNotification.GetText(WarningNotificationType.EquipInCombatIsImpossible));
			});
			return;
		}
		if (RootUIContext.Instance.IsChargenShown)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInLevlUpIsImpossible, null, addToLog: false);
			});
			return;
		}
		if (to is EquipSlotVM equipSlotVM)
		{
			if (!equipSlotVM.ItemSlot.Owner.CanBeControlled())
			{
				return;
			}
			if (from.Group != to.Group)
			{
				if (!equipSlotVM.ItemSlot.CanInsertItem(from.Item.CurrentValue))
				{
					EventBus.RaiseEvent(delegate(IInventoryNotificationHandler h)
					{
						h.HandleWarning(UIStrings.Instance.InventoryScreen.CantInsertThisWeapon.Text);
					});
				}
				Game.Instance.GameCommandQueue.EquipItem(from.Item.CurrentValue, equipSlotVM.ItemSlot.Owner, equipSlotVM.ToSlotRef());
				return;
			}
		}
		if (from is EquipSlotVM equipSlotVM2)
		{
			if (!equipSlotVM2.ItemSlot.Owner.CanBeControlled())
			{
				return;
			}
			if (from.Group != to.Group)
			{
				MechanicEntity owner = equipSlotVM2.ItemSlot.Owner;
				Game.Instance.GameCommandQueue.UnequipItem(owner, equipSlotVM2.ToSlotRef(), to.ToSlotRef());
				return;
			}
		}
		ProcessDragEnd(from, to, isLootOrCargo: false, fromShip);
	}

	private static void ProcessDragEnd(ItemSlotVM from, ItemSlotVM to, bool isLootOrCargo, bool fromShip)
	{
		ItemSlotRef fromRef = from.ToSlotRef();
		ItemSlotRef toRef = to.ToSlotRef();
		switch (GetEndDragAction(from, to))
		{
		case EndDragAction.Put:
		case EndDragAction.Swap:
			if (from.Group == to.Group && from.Group != null && !fromShip)
			{
				from.Group.SetSorterType(ItemsSorterType.NotSorted);
			}
			Game.Instance.GameCommandQueue.SwapSlots(from.ItemEntity.Owner, fromRef, toRef, isLootOrCargo);
			break;
		case EndDragAction.Split:
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Split, from.Item.CurrentValue, delegate(int count)
				{
					Game.Instance.GameCommandQueue.SplitSlot(fromRef, toRef, isLootOrCargo, count);
				});
			});
			break;
		case EndDragAction.HalfSplit:
			Game.Instance.GameCommandQueue.SplitSlot(fromRef, toRef, isLootOrCargo, from.Item.CurrentValue.Count / 2);
			break;
		case EndDragAction.Merge:
			Game.Instance.GameCommandQueue.MergeSlot(fromRef, toRef);
			break;
		case EndDragAction.Abort:
			break;
		}
	}

	private static EndDragAction GetEndDragAction(ItemSlotVM from, ItemSlotVM to)
	{
		if (from == to)
		{
			return EndDragAction.Abort;
		}
		if (to.HasItem && to.Item.CurrentValue.CanBeMerged(from.Item.CurrentValue))
		{
			return EndDragAction.Merge;
		}
		if (from.Item.CurrentValue.IsStackable && from.Item.CurrentValue.Count > 1 && !to.HasItem)
		{
			if (KeyboardAccess.IsShiftHold())
			{
				return EndDragAction.Split;
			}
			if (KeyboardAccess.IsCtrlHold())
			{
				return EndDragAction.HalfSplit;
			}
		}
		if (!to.HasItem)
		{
			return EndDragAction.Put;
		}
		return EndDragAction.Swap;
	}

	public static bool TryCollectLootSlot(ItemSlotVM slot)
	{
		if (slot == null || !slot.HasItem)
		{
			return false;
		}
		Game.Instance.GameCommandQueue.CollectLoot(new List<EntityRef<ItemEntity>>
		{
			new EntityRef<ItemEntity>(slot.ItemEntity)
		});
		return true;
	}

	public static void InsertToInteractionSlot(InsertableLootSlotVM fromSlot, InteractionSlotPartVM toSlot)
	{
		if (toSlot != null)
		{
			ItemsCollection obj = fromSlot.ItemEntity?.Collection ?? fromSlot.Group.MechanicCollection;
			ItemsCollection mechanicCollection = toSlot.Group.MechanicCollection;
			if (obj != null && mechanicCollection != null && fromSlot.CanInsert.CurrentValue)
			{
				TryCollectLootSlot(toSlot.ItemSlot.CurrentValue);
				Game.Instance.GameCommandQueue.TransferItem(fromSlot.ItemEntity, mechanicCollection.ToCollectionRef(), 1);
			}
		}
	}

	public static void TryMoveSlot(ItemSlotVM from, ItemSlotVM to, InteractionSlotPartVM interactionSlot)
	{
		if (from.Group != to.Group && from is InsertableLootSlotVM fromSlot)
		{
			InsertToInteractionSlot(fromSlot, interactionSlot);
		}
		else if (from.Group == to.Group || from.HasItem)
		{
			ProcessDragEnd(from, to, isLootOrCargo: true, fromShip: false);
		}
	}

	public static void TryTransferInventorySlot(ItemSlotVM slot, LootObjectVM contextLoot, int count = 0)
	{
		SlotsGroupVM<ItemSlotVM> slotsGroupVM = contextLoot?.SlotsGroup;
		if (slotsGroupVM == null)
		{
			return;
		}
		ItemEntity itemEntity = slot.ItemEntity;
		if (itemEntity != null)
		{
			ItemsCollection mechanicCollection = slotsGroupVM.MechanicCollection;
			if (itemEntity.Collection != mechanicCollection)
			{
				ItemsCollectionRef to = mechanicCollection.ToCollectionRef();
				Game.Instance.GameCommandQueue.TransferItem(itemEntity, to, (count == 0) ? itemEntity.Count : count);
			}
		}
	}
}
