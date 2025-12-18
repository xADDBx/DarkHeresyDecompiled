using System;

namespace ObservableCollections;

public interface IFreezedCollection<T>
{
	ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool reverse = false);

	ISortableSynchronizedView<T, TView> CreateSortableView<TView>(Func<T, TView> transform);
}
