using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.Utility.GameConst;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootObjectVM : VirtualListElementVMBase
{
	public readonly LootObjectType Type;

	public readonly string DisplayName;

	public readonly string Description;

	public readonly Sprite DisplayIcon;

	public readonly SlotsGroupVM<ItemSlotVM> SlotsGroup;

	public IEnumerable<ItemEntity> ItemEntities => SlotsGroup.Items;

	public IEnumerable<ItemEntity> LootableItems => ItemEntities.Where((ItemEntity k) => k.IsLootable);

	public bool HasLootableItems => LootableItems.Any();

	public LootObjectVM(LootObjectType type, string displayName, string description, Sprite icon, ItemsCollection itemsCollection, IEnumerable<ItemEntity> items, LootWindowMode mode)
	{
		Type = type;
		DisplayName = displayName;
		Description = description;
		DisplayIcon = icon;
		if (mode == LootWindowMode.PlayerChest)
		{
			SlotsGroup = new ItemSlotsGroupVM(itemsCollection, items, 12, 84, new ItemsSortingContext
			{
				SorterType = ItemsSorterType.NameUp
			}, showUnavailableItems: true, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory);
			AddDisposable(SlotsGroup);
		}
		else
		{
			ItemsSortingContext sortingCtx = new ItemsSortingContext
			{
				SorterType = ItemsSorterType.DateUp,
				RemoveEmptySlots = true
			};
			SlotsGroup = new ItemSlotsGroupVM(itemsCollection, items, (mode == LootWindowMode.Short) ? UIConsts.MinLootSlotsInRow : UIConsts.MinLootSlotsInRowZoneExit, (mode == LootWindowMode.Short) ? UIConsts.MinLootSlotsInShort : UIConsts.MinLootSlotsInZoneExit, sortingCtx, showUnavailableItems: true, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Loot);
			AddDisposable(SlotsGroup);
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetNewItems(IEnumerable<ItemEntity> items)
	{
		SlotsGroup.SetNewItems(items);
	}

	public bool ContainsSlot(ItemSlotVM slot)
	{
		return SlotsGroup.VisibleCollection.Contains(slot);
	}
}
