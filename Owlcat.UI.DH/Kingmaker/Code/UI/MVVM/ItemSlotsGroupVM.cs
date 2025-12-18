using System;
using System.Collections.Generic;
using Kingmaker.Items;

namespace Kingmaker.Code.UI.MVVM;

public class ItemSlotsGroupVM : SlotsGroupVM<ItemSlotVM>
{
	public ItemSlotsGroupVM(ItemsCollection collection, int slotsInRow, int minSlots, ItemsSortingContext sortingCtx = default(ItemsSortingContext), bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, int? maxSlots = null)
		: base(collection, slotsInRow, minSlots, maxSlots, (IEnumerable<ItemEntity>)null, sortingCtx, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null)
	{
	}

	public ItemSlotsGroupVM(ItemsCollection collection, IEnumerable<ItemEntity> items, int slotsInRow, int minSlots, ItemsSortingContext sortingCtx = default(ItemsSortingContext), bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown)
		: base(collection, slotsInRow, minSlots, (int?)null, items, sortingCtx, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null)
	{
	}

	protected override ItemSlotVM GetItem(ItemEntity item, int index)
	{
		return new ItemSlotVM(item, index, this);
	}
}
