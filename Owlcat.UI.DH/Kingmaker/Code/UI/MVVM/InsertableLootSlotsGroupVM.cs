using System;
using System.Collections.Generic;
using Kingmaker.Items;

namespace Kingmaker.Code.UI.MVVM;

public class InsertableLootSlotsGroupVM : SlotsGroupVM<InsertableLootSlotVM>
{
	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	private Func<ItemEntity, bool> CanInsert => (ItemEntity entity) => m_CanInsertItem?.Invoke(entity) ?? true;

	public InsertableLootSlotsGroupVM(ItemsCollection collection, Func<ItemEntity, bool> canInsertItem, int slotsInRow, int minSlots, ItemsSortingContext sortingCtx = default(ItemsSortingContext), bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown)
		: base(collection, slotsInRow, minSlots, (int?)12, (IEnumerable<ItemEntity>)null, sortingCtx, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null)
	{
		m_CanInsertItem = canInsertItem;
		base.VisibleCollection.ForEach(delegate(InsertableLootSlotVM item)
		{
			item.UpdateCanInsert();
		});
	}

	protected override InsertableLootSlotVM GetItem(ItemEntity item, int index)
	{
		return new InsertableLootSlotVM(item, index, this, CanInsert);
	}

	protected override bool ShouldShowItem(ItemEntity item)
	{
		if (base.ShouldShowItem(item))
		{
			return CanInsert(item);
		}
		return false;
	}
}
