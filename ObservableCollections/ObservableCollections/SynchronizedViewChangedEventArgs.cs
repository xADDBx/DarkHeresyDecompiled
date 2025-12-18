using System.Collections.Specialized;

namespace ObservableCollections;

public readonly struct SynchronizedViewChangedEventArgs<T, TView>
{
	public readonly NotifyCollectionChangedAction Action;

	public readonly T NewValue;

	public readonly T OldValue;

	public readonly TView NewView;

	public readonly TView OldView;

	public readonly int NewViewIndex;

	public readonly int OldViewIndex;

	public SynchronizedViewChangedEventArgs(NotifyCollectionChangedAction action, T newValue = default(T), T oldValue = default(T), TView newView = default(TView), TView oldView = default(TView), int newViewIndex = -1, int oldViewIndex = -1)
	{
		Action = action;
		NewValue = newValue;
		OldValue = oldValue;
		NewView = newView;
		OldView = oldView;
		NewViewIndex = newViewIndex;
		OldViewIndex = oldViewIndex;
	}
}
