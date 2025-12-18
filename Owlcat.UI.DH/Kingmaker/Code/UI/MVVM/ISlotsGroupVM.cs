using System;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public interface ISlotsGroupVM : IDisposable
{
	ItemsCollection MechanicCollection { get; }

	ReadOnlyReactiveProperty<ItemsFilterType> FilterType { get; }

	ReadOnlyReactiveProperty<ItemsSorterType> SorterType { get; }

	ReadOnlyReactiveProperty<bool> ShowUnavailable { get; }

	ReadOnlyReactiveProperty<string> SearchString { get; }

	ItemSlotsGroupType Type { get; }

	Observable<Unit> CollectionChangedCommand { get; }

	void UpdateVisibleCollection();

	void SetFilterType(ItemsFilterType filter);

	void SetSorterType(ItemsSorterType sorter);

	void SetSearchQuery(string query);

	void SetShowUnavailable(bool show);

	void TriggerCollectionChanged();
}
public interface ISlotsGroupVM<out TViewModel, TItem> : ISlotsGroupVM, IDisposable
{
	TViewModel GetFirstEmptySlot();

	bool TryGetValidItem(Func<TItem, bool> predicate, out TItem result);

	TViewModel GetVisibleElementOrDefault(Func<TViewModel, bool> predicate);
}
