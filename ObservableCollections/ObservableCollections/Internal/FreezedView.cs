using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ObservableCollections.Internal;

internal sealed class FreezedView<T, TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
{
	private readonly bool reverse;

	private readonly List<(T, TView)> list;

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
				return list.Count;
			}
		}
	}

	public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

	public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

	public FreezedView(IEnumerable<T> source, Func<T, TView> selector, bool reverse)
	{
		Func<T, TView> selector = selector;
		base._002Ector();
		this.reverse = reverse;
		filter = SynchronizedViewFilter<T, TView>.Null;
		list = source.Select<T, (T, TView)>((T x) => (x: x, selector(x))).ToList();
	}

	public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
	{
		lock (SyncRoot)
		{
			this.filter = filter;
			for (int i = 0; i < list.Count; i++)
			{
				var (value, view) = list[i];
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
			if (resetAction == null)
			{
				return;
			}
			foreach (var (arg, arg2) in list)
			{
				resetAction(arg, arg2);
			}
		}
	}

	public IEnumerator<(T, TView)> GetEnumerator()
	{
		lock (SyncRoot)
		{
			if (!reverse)
			{
				foreach (var item in list)
				{
					if (filter.IsMatch(item.Item1, item.Item2))
					{
						yield return item;
					}
				}
				yield break;
			}
			foreach (var item2 in list.AsEnumerable().Reverse())
			{
				if (filter.IsMatch(item2.Item1, item2.Item2))
				{
					yield return item2;
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

	public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged()
	{
		return new NotifyCollectionChangedSynchronizedView<T, TView>(this);
	}
}
