using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ObservableCollections.Internal;

internal class SortedViewViewComparer<T, TKey, TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable where TKey : notnull
{
	private sealed class Comparer : IComparer<(TView view, TKey id)>
	{
		private readonly IComparer<TView> comparer;

		public Comparer(IComparer<TView> comparer)
		{
			this.comparer = comparer;
		}

		public int Compare((TView view, TKey id) x, (TView view, TKey id) y)
		{
			int num = comparer.Compare(x.view, y.view);
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

	private readonly Dictionary<TKey, TView> viewMap;

	private readonly SortedList<(TView View, TKey Key), (T Value, TView View)> list;

	private ISynchronizedViewFilter<T, TView> filter;

	public object SyncRoot { get; } = new object();


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

	public SortedViewViewComparer(IObservableCollection<T> source, Func<T, TKey> identitySelector, Func<T, TView> transform, IComparer<TView> comparer)
	{
		this.source = source;
		this.identitySelector = identitySelector;
		this.transform = transform;
		filter = SynchronizedViewFilter<T, TView>.Null;
		lock (source.SyncRoot)
		{
			Dictionary<(TView, TKey), (T, TView)> dictionary = new Dictionary<(TView, TKey), (T, TView)>(source.Count);
			viewMap = new Dictionary<TKey, TView>();
			foreach (T item in source)
			{
				TView val = transform(item);
				TKey val2 = identitySelector(item);
				dictionary.Add((val, val2), (item, val));
				viewMap.Add(val2, val);
			}
			list = new SortedList<(TView, TKey), (T, TView)>(dictionary, new Comparer(comparer));
			this.source.CollectionChanged += SourceCollectionChanged;
		}
	}

	public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
	{
		lock (SyncRoot)
		{
			this.filter = filter;
			int num = 0;
			foreach (KeyValuePair<(TView, TKey), (T, TView)> item in list)
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
			foreach (KeyValuePair<(TView, TKey), (T, TView)> item in list)
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
			foreach (KeyValuePair<(TView, TKey), (T, TView)> item in list)
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
					T newItem = e.NewItem;
					TView val2 = transform(newItem);
					TKey val3 = identitySelector(newItem);
					list.Add((val2, val3), (newItem, val2));
					viewMap.Add(val3, val2);
					int index = list.IndexOfKey((val2, val3));
					filter.InvokeOnAdd(newItem, val2, index);
					break;
				}
				ReadOnlySpan<T> newItems = e.NewItems;
				for (int i = 0; i < newItems.Length; i++)
				{
					T val4 = newItems[i];
					TView val5 = transform(val4);
					TKey val6 = identitySelector(val4);
					list.Add((val5, val6), (val4, val5));
					viewMap.Add(val6, val5);
					int index2 = list.IndexOfKey((val5, val6));
					filter.InvokeOnAdd(val4, val5, index2);
				}
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				if (e.IsSingleItem)
				{
					T oldItem3 = e.OldItem;
					TKey val10 = identitySelector(oldItem3);
					if (viewMap.Remove(val10, out var value4))
					{
						(TView, TKey) key2 = (value4, val10);
						if (list.TryGetValue(key2, out (T, TView) value5))
						{
							int num3 = list.IndexOfKey(key2);
							list.RemoveAt(num3);
							filter.InvokeOnRemove(value5, num3);
						}
					}
					break;
				}
				ReadOnlySpan<T> newItems = e.OldItems;
				for (int i = 0; i < newItems.Length; i++)
				{
					T arg = newItems[i];
					TKey val11 = identitySelector(arg);
					if (viewMap.Remove(val11, out var value6))
					{
						(TView, TKey) key3 = (value6, val11);
						if (list.TryGetValue(key3, out (T, TView) value7))
						{
							int num4 = list.IndexOfKey((value6, val11));
							list.RemoveAt(num4);
							filter.InvokeOnRemove(value7, num4);
						}
					}
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
			{
				T oldItem2 = e.OldItem;
				TKey val7 = identitySelector(oldItem2);
				int num2 = -1;
				if (viewMap.Remove(val7, out var value2))
				{
					(TView, TKey) key = (value2, val7);
					if (list.TryGetValue(key, out (T, TView) _))
					{
						num2 = list.IndexOfKey(key);
						list.RemoveAt(num2);
					}
				}
				T newItem2 = e.NewItem;
				TView val8 = transform(newItem2);
				TKey val9 = identitySelector(newItem2);
				list.Add((val8, val9), (newItem2, val8));
				viewMap.Add(val9, val8);
				int index3 = list.IndexOfKey((val8, val9));
				filter.InvokeOnReplace(newItem2, val8, oldItem2, value2, index3, num2);
				break;
			}
			case NotifyCollectionChangedAction.Move:
			{
				T oldItem = e.OldItem;
				TKey val = identitySelector(oldItem);
				if (viewMap.TryGetValue(val, out var value))
				{
					int num = list.IndexOfKey((value, val));
					filter.InvokeOnMove(oldItem, value, num, num);
				}
				break;
			}
			case NotifyCollectionChangedAction.Reset:
				list.Clear();
				viewMap.Clear();
				filter.InvokeOnReset();
				break;
			}
			this.RoutingCollectionChanged?.Invoke(in e);
			this.CollectionStateChanged?.Invoke(e.Action);
		}
	}
}
