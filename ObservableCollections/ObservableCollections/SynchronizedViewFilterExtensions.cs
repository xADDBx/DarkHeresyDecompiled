using System;
using System.Collections.Specialized;

namespace ObservableCollections;

public static class SynchronizedViewFilterExtensions
{
	public static void AttachFilter<T, TView>(this ISynchronizedView<T, TView> source, Func<T, TView, bool> filter)
	{
		source.AttachFilter(new SynchronizedViewFilter<T, TView>(filter, null, null, null));
	}

	public static void AttachFilter<T, TView>(this ISynchronizedView<T, TView> source, Func<T, TView, bool> isMatch, Action<T, TView>? whenTrue, Action<T, TView>? whenFalse)
	{
		source.AttachFilter(new SynchronizedViewFilter<T, TView>(isMatch, whenTrue, whenFalse, null));
	}

	public static void AttachFilter<T, TView>(this ISynchronizedView<T, TView> source, Func<T, TView, bool> isMatch, Action<T, TView>? whenTrue, Action<T, TView>? whenFalse, Action<SynchronizedViewChangedEventArgs<T, TView>>? onCollectionChanged)
	{
		source.AttachFilter(new SynchronizedViewFilter<T, TView>(isMatch, whenTrue, whenFalse, onCollectionChanged));
	}

	public static bool IsNullFilter<T, TView>(this ISynchronizedViewFilter<T, TView> filter)
	{
		return filter == SynchronizedViewFilter<T, TView>.Null;
	}

	internal static void InvokeOnAdd<T, TView>(this ISynchronizedViewFilter<T, TView> filter, (T value, TView view) value, int index)
	{
		filter.InvokeOnAdd(value.value, value.view, index);
	}

	internal static void InvokeOnAdd<T, TView>(this ISynchronizedViewFilter<T, TView> filter, T value, TView view, int index)
	{
		if (filter.IsMatch(value, view))
		{
			filter.WhenTrue(value, view);
		}
		else
		{
			filter.WhenFalse(value, view);
		}
		SynchronizedViewChangedEventArgs<T, TView> eventArgs = new SynchronizedViewChangedEventArgs<T, TView>(NotifyCollectionChangedAction.Add, value, default(T), view, default(TView), index);
		filter.OnCollectionChanged(in eventArgs);
	}

	internal static void InvokeOnRemove<T, TView>(this ISynchronizedViewFilter<T, TView> filter, (T value, TView view) value, int oldIndex)
	{
		filter.InvokeOnRemove(value.value, value.view, oldIndex);
	}

	internal static void InvokeOnRemove<T, TView>(this ISynchronizedViewFilter<T, TView> filter, T value, TView view, int oldIndex)
	{
		SynchronizedViewChangedEventArgs<T, TView> eventArgs = new SynchronizedViewChangedEventArgs<T, TView>(NotifyCollectionChangedAction.Remove, default(T), value, default(TView), view, -1, oldIndex);
		filter.OnCollectionChanged(in eventArgs);
	}

	internal static void InvokeOnMove<T, TView>(this ISynchronizedViewFilter<T, TView> filter, (T value, TView view) value, int index, int oldIndex)
	{
		filter.InvokeOnMove(value.value, value.view, index, oldIndex);
	}

	internal static void InvokeOnMove<T, TView>(this ISynchronizedViewFilter<T, TView> filter, T value, TView view, int index, int oldIndex)
	{
		SynchronizedViewChangedEventArgs<T, TView> eventArgs = new SynchronizedViewChangedEventArgs<T, TView>(NotifyCollectionChangedAction.Move, value, default(T), view, default(TView), index, oldIndex);
		filter.OnCollectionChanged(in eventArgs);
	}

	internal static void InvokeOnReplace<T, TView>(this ISynchronizedViewFilter<T, TView> filter, (T value, TView view) value, (T value, TView view) oldValue, int index, int oldIndex = -1)
	{
		filter.InvokeOnReplace(value.value, value.view, oldValue.value, oldValue.view, index, oldIndex);
	}

	internal static void InvokeOnReplace<T, TView>(this ISynchronizedViewFilter<T, TView> filter, T value, TView view, T oldValue, TView oldView, int index, int oldIndex = -1)
	{
		SynchronizedViewChangedEventArgs<T, TView> eventArgs = new SynchronizedViewChangedEventArgs<T, TView>(NotifyCollectionChangedAction.Replace, value, oldValue, view, oldView, index, (oldIndex >= 0) ? oldIndex : index);
		filter.OnCollectionChanged(in eventArgs);
	}

	internal static void InvokeOnReset<T, TView>(this ISynchronizedViewFilter<T, TView> filter)
	{
		SynchronizedViewChangedEventArgs<T, TView> eventArgs = new SynchronizedViewChangedEventArgs<T, TView>(NotifyCollectionChangedAction.Reset);
		filter.OnCollectionChanged(in eventArgs);
	}

	internal static void InvokeOnAttach<T, TView>(this ISynchronizedViewFilter<T, TView> filter, T value, TView view)
	{
		if (filter.IsMatch(value, view))
		{
			filter.WhenTrue(value, view);
		}
		else
		{
			filter.WhenFalse(value, view);
		}
	}
}
