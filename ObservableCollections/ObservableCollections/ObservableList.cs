using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IObservableCollection<T>
{
	private sealed class View<TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly ObservableList<T> source;

		private readonly Func<T, TView> selector;

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

		public object SyncRoot { get; }

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

		public View(ObservableList<T> source, Func<T, TView> selector, bool reverse)
		{
			Func<T, TView> selector = selector;
			base._002Ector();
			this.source = source;
			this.selector = selector;
			this.reverse = reverse;
			filter = SynchronizedViewFilter<T, TView>.Null;
			SyncRoot = new object();
			lock (source.SyncRoot)
			{
				list = source.list.Select<T, (T, TView)>((T x) => (x: x, selector(x))).ToList();
				this.source.CollectionChanged += SourceCollectionChanged;
			}
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
			source.CollectionChanged -= SourceCollectionChanged;
		}

		private void SourceCollectionChanged(in NotifyCollectionChangedEventArgs<T> e)
		{
			lock (SyncRoot)
			{
				switch (e.Action)
				{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex == list.Count)
					{
						if (e.IsSingleItem)
						{
							(T, TView) tuple2 = (e.NewItem, selector(e.NewItem));
							list.Add(tuple2);
							filter.InvokeOnAdd(tuple2, e.NewStartingIndex);
							break;
						}
						int newStartingIndex = e.NewStartingIndex;
						ReadOnlySpan<T> newItems = e.NewItems;
						for (int j = 0; j < newItems.Length; j++)
						{
							T val = newItems[j];
							(T, TView) tuple3 = (val, selector(val));
							list.Add(tuple3);
							filter.InvokeOnAdd(tuple3, newStartingIndex++);
						}
					}
					else if (e.IsSingleItem)
					{
						(T, TView) tuple4 = (e.NewItem, selector(e.NewItem));
						list.Insert(e.NewStartingIndex, tuple4);
						filter.InvokeOnAdd(tuple4, e.NewStartingIndex);
					}
					else
					{
						(T, TView)[] array = new(T, TView)[e.NewItems.Length];
						ReadOnlySpan<T> newItems2 = e.NewItems;
						for (int k = 0; k < newItems2.Length; k++)
						{
							SynchronizedViewFilterExtensions.InvokeOnAdd(value: array[k] = (newItems2[k], selector(newItems2[k])), filter: filter, index: e.NewStartingIndex + k);
						}
						list.InsertRange(e.NewStartingIndex, array);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
				{
					if (e.IsSingleItem)
					{
						(T, TView) value2 = list[e.OldStartingIndex];
						list.RemoveAt(e.OldStartingIndex);
						filter.InvokeOnRemove(value2, e.OldStartingIndex);
						break;
					}
					int num = e.OldStartingIndex + e.OldItems.Length;
					for (int i = e.OldStartingIndex; i < num; i++)
					{
						(T, TView) value3 = list[i];
						filter.InvokeOnRemove(value3, e.OldStartingIndex + i);
					}
					list.RemoveRange(e.OldStartingIndex, e.OldItems.Length);
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				{
					(T, TView) value = (e.NewItem, selector(e.NewItem));
					(T, TView) oldValue = (e.OldItem, list[e.OldStartingIndex].Item2);
					list[e.NewStartingIndex] = value;
					filter.InvokeOnReplace(value, oldValue, e.NewStartingIndex);
					break;
				}
				case NotifyCollectionChangedAction.Move:
				{
					(T, TView) tuple = list[e.OldStartingIndex];
					list.RemoveAt(e.OldStartingIndex);
					list.Insert(e.NewStartingIndex, tuple);
					filter.InvokeOnMove(tuple, e.NewStartingIndex, e.OldStartingIndex);
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

	private readonly List<T> list;

	public object SyncRoot { get; } = new object();


	public T this[int index]
	{
		get
		{
			lock (SyncRoot)
			{
				return list[index];
			}
		}
		set
		{
			lock (SyncRoot)
			{
				T oldItem = list[index];
				list[index] = value;
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Replace(value, oldItem, index, index);
					collectionChanged(in e);
				}
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

	public bool IsReadOnly => false;

	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	public ObservableList()
	{
		list = new List<T>();
	}

	public ObservableList(int capacity)
	{
		list = new List<T>(capacity);
	}

	public ObservableList(IEnumerable<T> collection)
	{
		list = collection.ToList();
	}

	public void Add(T item)
	{
		lock (SyncRoot)
		{
			int count = list.Count;
			list.Add(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, count);
				collectionChanged(in e);
			}
		}
	}

	public void AddRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			int count = list.Count;
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			list.AddRange(cloneCollection.AsEnumerable());
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, count);
				collectionChanged(in e);
			}
		}
	}

	public void AddRange(T[] items)
	{
		lock (SyncRoot)
		{
			int count = list.Count;
			list.AddRange(items);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public void AddRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			int count = list.Count;
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				list.Add(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			list.Clear();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Reset();
				collectionChanged(in e);
			}
		}
	}

	public bool Contains(T item)
	{
		lock (SyncRoot)
		{
			return list.Contains(item);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		lock (SyncRoot)
		{
			list.CopyTo(array, arrayIndex);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (T item in list)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void ForEach(Action<T> action)
	{
		lock (SyncRoot)
		{
			foreach (T item in list)
			{
				action(item);
			}
		}
	}

	public int IndexOf(T item)
	{
		lock (SyncRoot)
		{
			return list.IndexOf(item);
		}
	}

	public void Insert(int index, T item)
	{
		lock (SyncRoot)
		{
			list.Insert(index, item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, index);
				collectionChanged(in e);
			}
		}
	}

	public void InsertRange(int index, T[] items)
	{
		lock (SyncRoot)
		{
			list.InsertRange(index, items);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, index);
				collectionChanged(in e);
			}
		}
	}

	public void InsertRange(int index, IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			list.InsertRange(index, cloneCollection.AsEnumerable());
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, index);
				collectionChanged(in e);
			}
		}
	}

	public void InsertRange(int index, ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			list.InsertRange(index, cloneCollection.AsEnumerable());
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, index);
				collectionChanged(in e);
			}
		}
	}

	public bool Remove(T item)
	{
		lock (SyncRoot)
		{
			int num = list.IndexOf(item);
			if (num >= 0)
			{
				list.RemoveAt(num);
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(item, num);
					collectionChanged(in e);
				}
				return true;
			}
			return false;
		}
	}

	public void RemoveAt(int index)
	{
		lock (SyncRoot)
		{
			T oldItem = list[index];
			list.RemoveAt(index);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(oldItem, index);
				collectionChanged(in e);
			}
		}
	}

	public void RemoveRange(int index, int count)
	{
		lock (SyncRoot)
		{
			List<T> range = list.GetRange(index, count);
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(range);
			list.RemoveRange(index, count);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(cloneCollection.Span, index);
				collectionChanged(in e);
			}
		}
	}

	public void Move(int oldIndex, int newIndex)
	{
		lock (SyncRoot)
		{
			T val = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, val);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Move(val, newIndex, oldIndex);
				collectionChanged(in e);
			}
		}
	}

	public ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool reverse = false)
	{
		return new View<TView>(this, transform, reverse);
	}
}
