using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ObservableCollections;

public interface ISynchronizedView<T, TView> : IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
{
	object SyncRoot { get; }

	ISynchronizedViewFilter<T, TView> CurrentFilter { get; }

	event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

	event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

	void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForInitialElements = false);

	void ResetFilter(Action<T, TView>? resetAction);

	INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged();
}
