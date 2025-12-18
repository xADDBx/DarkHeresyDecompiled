using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableRingBuffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IObservableCollection<T>
{
	internal sealed class View<TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly IObservableCollection<T> source;

		private readonly Func<T, TView> selector;

		private readonly bool reverse;

		private readonly RingBuffer<(T, TView)> ringBuffer;

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
					return ringBuffer.Count;
				}
			}
		}

		public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

		public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

		public View(IObservableCollection<T> source, Func<T, TView> selector, bool reverse)
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
				ringBuffer = new RingBuffer<(T, TView)>(source.Select<T, (T, TView)>((T x) => (x: x, selector(x))));
				this.source.CollectionChanged += SourceCollectionChanged;
			}
		}

		public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
		{
			lock (SyncRoot)
			{
				this.filter = filter;
				for (int i = 0; i < ringBuffer.Count; i++)
				{
					var (value, view) = ringBuffer[i];
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
				foreach (var (arg, arg2) in ringBuffer)
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
					foreach (var item in ringBuffer)
					{
						if (filter.IsMatch(item.Item1, item.Item2))
						{
							yield return item;
						}
					}
					yield break;
				}
				foreach (var item2 in ringBuffer.AsEnumerable().Reverse())
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
					if (e.NewStartingIndex == 0 && ringBuffer.Count != 0)
					{
						if (e.IsSingleItem)
						{
							(T, TView) tuple = (e.NewItem, selector(e.NewItem));
							ringBuffer.AddFirst(tuple);
							filter.InvokeOnAdd(tuple, 0);
							break;
						}
						ReadOnlySpan<T> newItems = e.NewItems;
						for (int k = 0; k < newItems.Length; k++)
						{
							T val = newItems[k];
							(T, TView) tuple2 = (val, selector(val));
							ringBuffer.AddFirst(tuple2);
							filter.InvokeOnAdd(tuple2, 0);
						}
					}
					else if (e.IsSingleItem)
					{
						(T, TView) tuple3 = (e.NewItem, selector(e.NewItem));
						ringBuffer.AddLast(tuple3);
						filter.InvokeOnAdd(tuple3, ringBuffer.Count - 1);
					}
					else
					{
						ReadOnlySpan<T> newItems = e.NewItems;
						for (int k = 0; k < newItems.Length; k++)
						{
							T val2 = newItems[k];
							(T, TView) tuple4 = (val2, selector(val2));
							ringBuffer.AddLast(tuple4);
							filter.InvokeOnAdd(tuple4, ringBuffer.Count - 1);
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == 0)
					{
						if (e.IsSingleItem)
						{
							(T, TView) value2 = ringBuffer.RemoveFirst();
							filter.InvokeOnRemove(value2, 0);
							break;
						}
						for (int i = 0; i < e.OldItems.Length; i++)
						{
							(T, TView) value3 = ringBuffer.RemoveFirst();
							filter.InvokeOnRemove(value3, 0);
						}
					}
					else if (e.IsSingleItem)
					{
						int oldIndex = ringBuffer.Count - 1;
						(T, TView) value4 = ringBuffer.RemoveLast();
						filter.InvokeOnRemove(value4, oldIndex);
					}
					else
					{
						for (int j = 0; j < e.OldItems.Length; j++)
						{
							int oldIndex2 = ringBuffer.Count - 1;
							(T, TView) value5 = ringBuffer.RemoveLast();
							filter.InvokeOnRemove(value5, oldIndex2);
						}
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					ringBuffer.Clear();
					filter.InvokeOnReset();
					break;
				case NotifyCollectionChangedAction.Replace:
				{
					(T, TView) oldValue = ringBuffer[e.OldStartingIndex];
					(T, TView) value = (e.NewItem, selector(e.NewItem));
					ringBuffer[e.NewStartingIndex] = value;
					filter.InvokeOnReplace(value, oldValue, e.NewStartingIndex);
					break;
				}
				}
				this.RoutingCollectionChanged?.Invoke(in e);
				this.CollectionStateChanged?.Invoke(e.Action);
			}
		}
	}

	private readonly RingBuffer<T> buffer;

	public bool IsReadOnly => false;

	public object SyncRoot { get; } = new object();


	public T this[int index]
	{
		get
		{
			lock (SyncRoot)
			{
				return buffer[index];
			}
		}
		set
		{
			lock (SyncRoot)
			{
				T oldItem = buffer[index];
				buffer[index] = value;
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
				return buffer.Count;
			}
		}
	}

	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	public ObservableRingBuffer()
	{
		buffer = new RingBuffer<T>();
	}

	public ObservableRingBuffer(IEnumerable<T> collection)
	{
		buffer = new RingBuffer<T>(collection);
	}

	public void AddFirst(T item)
	{
		lock (SyncRoot)
		{
			buffer.AddFirst(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, 0);
				collectionChanged(in e);
			}
		}
	}

	public void AddLast(T item)
	{
		lock (SyncRoot)
		{
			buffer.AddLast(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, buffer.Count - 1);
				collectionChanged(in e);
			}
		}
	}

	public T RemoveFirst()
	{
		lock (SyncRoot)
		{
			T val = buffer.RemoveFirst();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(val, 0);
				collectionChanged(in e);
			}
			return val;
		}
	}

	public T RemoveLast()
	{
		lock (SyncRoot)
		{
			int oldStartingIndex = buffer.Count - 1;
			T val = buffer.RemoveLast();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(val, oldStartingIndex);
				collectionChanged(in e);
			}
			return val;
		}
	}

	public void AddLastRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			int count = buffer.Count;
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			ReadOnlySpan<T> span = cloneCollection.Span;
			for (int i = 0; i < span.Length; i++)
			{
				T item = span[i];
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, count);
				collectionChanged(in e);
			}
		}
	}

	public void AddLastRange(T[] items)
	{
		lock (SyncRoot)
		{
			int count = buffer.Count;
			foreach (T item in items)
			{
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public void AddLastRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			int count = buffer.Count;
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public int IndexOf(T item)
	{
		lock (SyncRoot)
		{
			return buffer.IndexOf(item);
		}
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Add(T item)
	{
		AddLast(item);
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			buffer.Clear();
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
			return buffer.Contains(item);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		lock (SyncRoot)
		{
			buffer.CopyTo(array, arrayIndex);
		}
	}

	public T[] ToArray()
	{
		lock (SyncRoot)
		{
			return buffer.ToArray();
		}
	}

	public int BinarySearch(T item)
	{
		lock (SyncRoot)
		{
			return buffer.BinarySearch(item);
		}
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		lock (SyncRoot)
		{
			return buffer.BinarySearch(item, comparer);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (T item in buffer)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool reverse = false)
	{
		return new View<TView>(this, transform, reverse);
	}
}
