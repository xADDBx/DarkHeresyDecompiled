using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;

namespace Kingmaker.Code.UI.MVVM;

public static class InventoryHelper
{
	public static ItemSlot GetEquipSlot(this PartUnitBody partUnitBody, EquipSlotType type, int handSetIndex)
	{
		switch (type)
		{
		case EquipSlotType.Armor:
			return partUnitBody.Armor;
		case EquipSlotType.PrimaryHand:
			return partUnitBody.HandsEquipmentSets[handSetIndex].PrimaryHand;
		case EquipSlotType.SecondaryHand:
			return partUnitBody.HandsEquipmentSets[handSetIndex].SecondaryHand;
		case EquipSlotType.Belt:
			return partUnitBody.Belt;
		case EquipSlotType.Head:
			return partUnitBody.Head;
		case EquipSlotType.Feet:
			return partUnitBody.Feet;
		case EquipSlotType.Gloves:
			return partUnitBody.Gloves;
		case EquipSlotType.Neck:
			return partUnitBody.Neck;
		case EquipSlotType.Ring1:
			return partUnitBody.Ring1;
		case EquipSlotType.Ring2:
			return partUnitBody.Ring2;
		case EquipSlotType.Wrist:
			return partUnitBody.Wrist;
		case EquipSlotType.Shoulders:
			return partUnitBody.Shoulders;
		case EquipSlotType.Glasses:
			return partUnitBody.Glasses;
		case EquipSlotType.Shirt:
			return partUnitBody.Shirt;
		case EquipSlotType.QuickSlot1:
		case EquipSlotType.QuickSlot2:
		case EquipSlotType.QuickSlot3:
		case EquipSlotType.QuickSlot4:
		case EquipSlotType.QuickSlot5:
			return partUnitBody.QuickSlots[handSetIndex];
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public static ItemsCollectionRef ToCollectionRef(this ItemsCollection itemsCollection)
	{
		return new ItemsCollectionRef(itemsCollection);
	}
}
