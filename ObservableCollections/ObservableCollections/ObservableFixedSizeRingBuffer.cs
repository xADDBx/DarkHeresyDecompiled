using System;
using System.Collections;
using System.Collections.Generic;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableFixedSizeRingBuffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IObservableCollection<T>
{
	private readonly RingBuffer<T> buffer;

	private readonly int capacity;

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

	public int Capacity => capacity;

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

	public ObservableFixedSizeRingBuffer(int capacity)
	{
		this.capacity = capacity;
		buffer = new RingBuffer<T>(capacity);
	}

	public ObservableFixedSizeRingBuffer(int capacity, IEnumerable<T> collection)
	{
		this.capacity = capacity;
		buffer = new RingBuffer<T>(capacity);
		foreach (T item in collection)
		{
			if (capacity == buffer.Count)
			{
				buffer.RemoveFirst();
			}
			buffer.AddLast(item);
		}
	}

	public void AddFirst(T item)
	{
		lock (SyncRoot)
		{
			NotifyCollectionChangedEventArgs<T> e;
			if (capacity == buffer.Count)
			{
				T oldItem = buffer.RemoveLast();
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					e = NotifyCollectionChangedEventArgs<T>.Remove(oldItem, capacity - 1);
					collectionChanged(in e);
				}
			}
			buffer.AddFirst(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged2 = this.CollectionChanged;
			if (collectionChanged2 != null)
			{
				e = NotifyCollectionChangedEventArgs<T>.Add(item, 0);
				collectionChanged2(in e);
			}
		}
	}

	public void AddLast(T item)
	{
		lock (SyncRoot)
		{
			NotifyCollectionChangedEventArgs<T> e;
			if (capacity == buffer.Count)
			{
				T oldItem = buffer.RemoveFirst();
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					e = NotifyCollectionChangedEventArgs<T>.Remove(oldItem, 0);
					collectionChanged(in e);
				}
			}
			buffer.AddLast(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged2 = this.CollectionChanged;
			if (collectionChanged2 != null)
			{
				e = NotifyCollectionChangedEventArgs<T>.Add(item, buffer.Count - 1);
				collectionChanged2(in e);
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
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			NotifyCollectionChangedEventArgs<T> e;
			if (capacity <= buffer.Count + cloneCollection.Span.Length)
			{
				int num = Math.Min(buffer.Count, buffer.Count + cloneCollection.Span.Length - capacity);
				using ResizableArray<T> resizableArray = new ResizableArray<T>(num);
				for (int i = 0; i < num; i++)
				{
					resizableArray.Add(buffer.RemoveFirst());
				}
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					e = NotifyCollectionChangedEventArgs<T>.Remove(resizableArray.Span, 0);
					collectionChanged(in e);
				}
			}
			int count = buffer.Count;
			ReadOnlySpan<T> readOnlySpan = cloneCollection.Span;
			if (readOnlySpan.Length > capacity)
			{
				readOnlySpan = readOnlySpan.Slice(readOnlySpan.Length - capacity);
			}
			ReadOnlySpan<T> readOnlySpan2 = readOnlySpan;
			for (int j = 0; j < readOnlySpan2.Length; j++)
			{
				T item = readOnlySpan2[j];
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged2 = this.CollectionChanged;
			if (collectionChanged2 != null)
			{
				e = NotifyCollectionChangedEventArgs<T>.Add(readOnlySpan, count);
				collectionChanged2(in e);
			}
		}
	}

	public void AddLastRange(T[] items)
	{
		lock (SyncRoot)
		{
			NotifyCollectionChangedEventArgs<T> e;
			if (capacity <= buffer.Count + items.Length)
			{
				int num = Math.Min(buffer.Count, buffer.Count + items.Length - capacity);
				using ResizableArray<T> resizableArray = new ResizableArray<T>(num);
				for (int i = 0; i < num; i++)
				{
					resizableArray.Add(buffer.RemoveFirst());
				}
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					e = NotifyCollectionChangedEventArgs<T>.Remove(resizableArray.Span, 0);
					collectionChanged(in e);
				}
			}
			int count = buffer.Count;
			Span<T> span = items.AsSpan();
			if (span.Length > capacity)
			{
				span = span.Slice(span.Length - capacity);
			}
			Span<T> span2 = span;
			for (int j = 0; j < span2.Length; j++)
			{
				T item = span2[j];
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged2 = this.CollectionChanged;
			if (collectionChanged2 != null)
			{
				e = NotifyCollectionChangedEventArgs<T>.Add(span, count);
				collectionChanged2(in e);
			}
		}
	}

	public void AddLastRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			NotifyCollectionChangedEventArgs<T> e;
			if (capacity <= buffer.Count + items.Length)
			{
				int num = Math.Min(buffer.Count, buffer.Count + items.Length - capacity);
				using ResizableArray<T> resizableArray = new ResizableArray<T>(num);
				for (int i = 0; i < num; i++)
				{
					resizableArray.Add(buffer.RemoveFirst());
				}
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					e = NotifyCollectionChangedEventArgs<T>.Remove(resizableArray.Span, 0);
					collectionChanged(in e);
				}
			}
			int count = buffer.Count;
			ReadOnlySpan<T> readOnlySpan = items;
			if (readOnlySpan.Length > capacity)
			{
				readOnlySpan = readOnlySpan.Slice(readOnlySpan.Length - capacity);
			}
			ReadOnlySpan<T> readOnlySpan2 = readOnlySpan;
			for (int j = 0; j < readOnlySpan2.Length; j++)
			{
				T item = readOnlySpan2[j];
				buffer.AddLast(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged2 = this.CollectionChanged;
			if (collectionChanged2 != null)
			{
				e = NotifyCollectionChangedEventArgs<T>.Add(readOnlySpan, count);
				collectionChanged2(in e);
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
		return new ObservableRingBuffer<T>.View<TView>(this, transform, reverse);
	}
}
