using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ObservableCollections.Internal;

internal sealed class FreezedSortableView<T, TView> : ISortableSynchronizedView<T, TView>, ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
{
	private class TComparer : IComparer<(T, TView)>
	{
		private readonly IComparer<T> comparer;

		public TComparer(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public int Compare((T, TView) x, (T, TView) y)
		{
			return comparer.Compare(x.Item1, y.Item1);
		}
	}

	private class TViewComparer : IComparer<(T, TView)>
	{
		private readonly IComparer<TView> comparer;

		public TViewComparer(IComparer<TView> comparer)
		{
			this.comparer = comparer;
		}

		public int Compare((T, TView) x, (T, TView) y)
		{
			return comparer.Compare(x.Item2, y.Item2);
		}
	}

	private readonly (T, TView)[] array;

	private ISynchronizedViewFilter<T, TView> filter;

	public ISynchronizedViewFilter<T, TView> CurrentFilter
	{
		get
		{
			lock (SyncRoot)
			{
				return filter;
			}
		}
	}

	public object SyncRoot { get; } = new object();


	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return array.Length;
			}
		}
	}

	public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

	public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

	public FreezedSortableView(IEnumerable<T> source, Func<T, TView> selector)
	{
		Func<T, TView> selector = selector;
		base._002Ector();
		filter = SynchronizedViewFilter<T, TView>.Null;
		array = source.Select<T, (T, TView)>((T x) => (x: x, selector(x))).ToArray();
	}

	public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
	{
		lock (SyncRoot)
		{
			this.filter = filter;
			for (int i = 0; i < array.Length; i++)
			{
				var (value, view) = array[i];
				if (invokeAddEventForCurrentElements)
				{
					filter.InvokeOnAdd(value, view, i);
				}
				else
				{
					filter.InvokeOnAttach(value, view);
				}
			}
		}
	}

	public void ResetFilter(Action<T, TView>? resetAction)
	{
		lock (SyncRoot)
		{
			filter = SynchronizedViewFilter<T, TView>.Null;
			if (resetAction != null)
			{
				(T, TView)[] array = this.array;
				for (int i = 0; i < array.Length; i++)
				{
					var (arg, arg2) = array[i];
					resetAction(arg, arg2);
				}
			}
		}
	}

	public IEnumerator<(T, TView)> GetEnumerator()
	{
		lock (SyncRoot)
		{
			(T, TView)[] array = this.array;
			for (int i = 0; i < array.Length; i++)
			{
				(T, TView) tuple = array[i];
				if (filter.IsMatch(tuple.Item1, tuple.Item2))
				{
					yield return tuple;
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Dispose()
	{
	}

	public void Sort(IComparer<T> comparer)
	{
		Array.Sort(array, new TComparer(comparer));
	}

	public void Sort(IComparer<TView> viewComparer)
	{
		Array.Sort(array, new TViewComparer(viewComparer));
	}

	public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged()
	{
		return new NotifyCollectionChangedSynchronizedView<T, TView>(this);
	}
}
