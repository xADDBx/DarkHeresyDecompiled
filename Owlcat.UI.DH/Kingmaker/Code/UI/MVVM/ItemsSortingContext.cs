using System;
using Kingmaker.Items;
using Kingmaker.UI.Common;

namespace Kingmaker.Code.UI.MVVM;

public struct ItemsSortingContext
{
	public ItemsSorterType SorterType;

	public ItemsFilterType FilterType;

	public Func<ItemsSorterType, ItemsFilterType, Comparison<ItemEntity>> GetComparisonFunc;

	public bool RemoveEmptySlots;
}
