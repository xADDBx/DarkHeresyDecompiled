using System;
using System.Collections;
using System.Collections.Generic;

namespace ObservableCollections;

public interface IObservableCollection<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	object SyncRoot { get; }

	event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool reverse = false);
}
