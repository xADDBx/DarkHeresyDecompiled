using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace ObservableCollections;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct NotifyCollectionChangedEventArgs<T>
{
	public readonly NotifyCollectionChangedAction Action;

	public readonly bool IsSingleItem;

	public readonly T NewItem;

	public readonly T OldItem;

	public readonly ReadOnlySpan<T> NewItems;

	public readonly ReadOnlySpan<T> OldItems;

	public readonly int NewStartingIndex;

	public readonly int OldStartingIndex;

	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, bool isSingleItem, T newItem = default(T), T oldItem = default(T), ReadOnlySpan<T> newItems = default(ReadOnlySpan<T>), ReadOnlySpan<T> oldItems = default(ReadOnlySpan<T>), int newStartingIndex = -1, int oldStartingIndex = -1)
	{
		Action = action;
		IsSingleItem = isSingleItem;
		NewItem = newItem;
		OldItem = oldItem;
		NewItems = newItems;
		OldItems = oldItems;
		NewStartingIndex = newStartingIndex;
		OldStartingIndex = oldStartingIndex;
	}

	public static NotifyCollectionChangedEventArgs<T> Add(T newItem, int newStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, isSingleItem: true, newItem, default(T), default(ReadOnlySpan<T>), default(ReadOnlySpan<T>), newStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Add(ReadOnlySpan<T> newItems, int newStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, isSingleItem: false, default(T), default(T), newItems, default(ReadOnlySpan<T>), newStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Remove(T oldItem, int oldStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, isSingleItem: true, default(T), oldItem, default(ReadOnlySpan<T>), default(ReadOnlySpan<T>), -1, oldStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Remove(ReadOnlySpan<T> oldItems, int oldStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, isSingleItem: false, default(T), default(T), default(ReadOnlySpan<T>), oldItems, -1, oldStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Replace(T newItem, T oldItem, int newStartingIndex, int oldStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Replace, isSingleItem: true, newItem, oldItem, default(ReadOnlySpan<T>), default(ReadOnlySpan<T>), newStartingIndex, oldStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Replace(ReadOnlySpan<T> newItems, ReadOnlySpan<T> oldItems, int newStartingIndex, int oldStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Replace, isSingleItem: false, default(T), default(T), newItems, oldItems, newStartingIndex, oldStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Move(T changedItem, int newStartingIndex, int oldStartingIndex)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Move, isSingleItem: true, changedItem, changedItem, default(ReadOnlySpan<T>), default(ReadOnlySpan<T>), newStartingIndex, oldStartingIndex);
	}

	public static NotifyCollectionChangedEventArgs<T> Reset()
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset, isSingleItem: true);
	}
}
