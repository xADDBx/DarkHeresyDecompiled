using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ObservableCollections.Internal;

internal class SortedView<T, TKey, TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable where TKey : notnull
{
	private sealed class Comparer : IComparer<(T value, TKey id)>
	{
		private readonly IComparer<T> comparer;

		public Comparer(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public int Compare((T value, TKey id) x, (T value, TKey id) y)
		{
			int num = comparer.Compare(x.value, y.value);
			if (num == 0)
			{
				num = Comparer<TKey>.Default.Compare(x.id, y.id);
			}
			return num;
		}
	}

	private readonly IObservableCollection<T> source;

	private readonly Func<T, TView> transform;

	private readonly Func<T, TKey> identitySelector;

	private readonly SortedList<(T Value, TKey Key), (T Value, TView View)> list;

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

	public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

	public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

	public SortedView(IObservableCollection<T> source, Func<T, TKey> identitySelector, Func<T, TView> transform, IComparer<T> comparer)
	{
		this.source = source;
		this.identitySelector = identitySelector;
		this.transform = transform;
		filter = SynchronizedViewFilter<T, TView>.Null;
		lock (source.SyncRoot)
		{
			Dictionary<(T, TKey), (T, TView)> dictionary = new Dictionary<(T, TKey), (T, TView)>(source.Count);
			foreach (T item in source)
			{
				dictionary.Add((item, identitySelector(item)), (item, transform(item)));
			}
			list = new SortedList<(T, TKey), (T, TView)>(dictionary, new Comparer(comparer));
			this.source.CollectionChanged += SourceCollectionChanged;
		}
	}

	public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
	{
		lock (SyncRoot)
		{
			this.filter = filter;
			int num = 0;
			foreach (KeyValuePair<(T, TKey), (T, TView)> item in list)
			{
				item.Deconstruct(out var _, out var value);
				var (value2, view) = value;
				if (invokeAddEventForCurrentElements)
				{
					filter.InvokeOnAdd(value2, view, num++);
				}
				else
				{
					filter.InvokeOnAttach(value2, view);
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
			foreach (KeyValuePair<(T, TKey), (T, TView)> item in list)
			{
				item.Deconstruct(out var _, out var value);
				var (arg, arg2) = value;
				resetAction(arg, arg2);
			}
		}
	}

	public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged()
	{
		lock (SyncRoot)
		{
			return new NotifyCollectionChangedSynchronizedView<T, TView>(this);
		}
	}

	public IEnumerator<(T, TView)> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (KeyValuePair<(T, TKey), (T, TView)> item in list)
			{
				if (filter.IsMatch(item.Value.Item1, item.Value.Item2))
				{
					yield return item.Value;
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
		source.CollectionChanged -= SourceCollectionChanged;
	}

	private void SourceCollectionChanged(in NotifyCollectionChangedEventArgs<T> e)
	{
		lock (SyncRoot)
		{
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
			{
				if (e.IsSingleItem)
				{
					T newItem2 = e.NewItem;
					TView val2 = transform(newItem2);
					TKey item5 = identitySelector(newItem2);
					list.Add((newItem2, item5), (newItem2, val2));
					int index2 = list.IndexOfKey((newItem2, item5));
					filter.InvokeOnAdd(newItem2, val2, index2);
					break;
				}
				ReadOnlySpan<T> oldItems = e.NewItems;
				for (int i = 0; i < oldItems.Length; i++)
				{
					T val3 = oldItems[i];
					TView val4 = transform(val3);
					TKey item6 = identitySelector(val3);
					list.Add((val3, item6), (val3, val4));
					int index3 = list.IndexOfKey((val3, item6));
					filter.InvokeOnAdd(val3, val4, index3);
				}
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				if (e.IsSingleItem)
				{
					T oldItem2 = e.OldItem;
					TKey item = identitySelector(oldItem2);
					(T, TKey) key2 = (oldItem2, item);
					if (list.TryGetValue(key2, out (T, TView) value2))
					{
						int num2 = list.IndexOfKey(key2);
						list.RemoveAt(num2);
						filter.InvokeOnRemove(value2.Item1, value2.Item2, num2);
					}
					break;
				}
				ReadOnlySpan<T> oldItems = e.OldItems;
				for (int i = 0; i < oldItems.Length; i++)
				{
					T val = oldItems[i];
					TKey item2 = identitySelector(val);
					(T, TKey) key3 = (val, item2);
					if (list.TryGetValue(key3, out (T, TView) value3))
					{
						int num3 = list.IndexOfKey((val, item2));
						list.RemoveAt(num3);
						filter.InvokeOnRemove(value3.Item1, value3.Item2, num3);
					}
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
			{
				T oldItem3 = e.OldItem;
				(T, TKey) key4 = (oldItem3, identitySelector(oldItem3));
				int num4 = -1;
				if (list.TryGetValue(key4, out (T, TView) value4))
				{
					num4 = list.IndexOfKey(key4);
					list.RemoveAt(num4);
				}
				T newItem = e.NewItem;
				TView item3 = transform(newItem);
				TKey item4 = identitySelector(newItem);
				list.Add((newItem, item4), (newItem, item3));
				int index = list.IndexOfKey((newItem, item4));
				filter.InvokeOnReplace((value: newItem, view: item3), value4, index, num4);
				break;
			}
			case NotifyCollectionChangedAction.Move:
			{
				T oldItem = e.OldItem;
				(T, TKey) key = (oldItem, identitySelector(oldItem));
				if (list.TryGetValue(key, out (T, TView) value))
				{
					int num = list.IndexOfKey(key);
					filter.InvokeOnMove(value, num, num);
				}
				break;
			}
			case NotifyCollectionChangedAction.Reset:
				list.Clear();
				filter.InvokeOnReset();
				break;
			}
			this.RoutingCollectionChanged?.Invoke(in e);
			this.CollectionStateChanged?.Invoke(e.Action);
		}
	}
}
