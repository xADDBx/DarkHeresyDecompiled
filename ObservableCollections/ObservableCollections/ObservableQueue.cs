using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableQueue<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IObservableCollection<T>
{
	private class View<TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly ObservableQueue<T> source;

		private readonly Func<T, TView> selector;

		private readonly bool reverse;

		protected readonly Queue<(T, TView)> queue;

		private ISynchronizedViewFilter<T, TView> filter;

		public object SyncRoot { get; }

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
					return queue.Count;
				}
			}
		}

		public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

		public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

		public View(ObservableQueue<T> source, Func<T, TView> selector, bool reverse)
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
				queue = new Queue<(T, TView)>(source.queue.Select<T, (T, TView)>((T x) => (x: x, selector(x))));
				this.source.CollectionChanged += SourceCollectionChanged;
			}
		}

		public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
		{
			lock (SyncRoot)
			{
				this.filter = filter;
				int num = 0;
				foreach (var (value, view) in queue)
				{
					if (invokeAddEventForCurrentElements)
					{
						filter.InvokeOnAdd(value, view, num++);
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
				foreach (var (arg, arg2) in queue)
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
					foreach (var item in queue)
					{
						if (filter.IsMatch(item.Item1, item.Item2))
						{
							yield return item;
						}
					}
					yield break;
				}
				foreach (var item2 in queue.AsEnumerable().Reverse())
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
				{
					if (e.IsSingleItem)
					{
						(T, TView) tuple3 = (e.NewItem, selector(e.NewItem));
						queue.Enqueue(tuple3);
						filter.InvokeOnAdd(tuple3, e.NewStartingIndex);
						break;
					}
					int newStartingIndex = e.NewStartingIndex;
					ReadOnlySpan<T> newItems = e.NewItems;
					for (int j = 0; j < newItems.Length; j++)
					{
						T val = newItems[j];
						(T, TView) tuple4 = (val, selector(val));
						queue.Enqueue(tuple4);
						filter.InvokeOnAdd(tuple4, newStartingIndex++);
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					if (e.IsSingleItem)
					{
						(T, TView) tuple = queue.Dequeue();
						filter.InvokeOnRemove(tuple.Item1, tuple.Item2, 0);
						break;
					}
					int length = e.OldItems.Length;
					for (int i = 0; i < length; i++)
					{
						(T, TView) tuple2 = queue.Dequeue();
						filter.InvokeOnRemove(tuple2.Item1, tuple2.Item2, 0);
					}
					break;
				}
				case NotifyCollectionChangedAction.Reset:
					queue.Clear();
					filter.InvokeOnReset();
					break;
				}
				this.RoutingCollectionChanged?.Invoke(in e);
				this.CollectionStateChanged?.Invoke(e.Action);
			}
		}
	}

	private readonly Queue<T> queue;

	public object SyncRoot { get; } = new object();


	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return queue.Count;
			}
		}
	}

	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	public ObservableQueue()
	{
		queue = new Queue<T>();
	}

	public ObservableQueue(int capacity)
	{
		queue = new Queue<T>(capacity);
	}

	public ObservableQueue(IEnumerable<T> collection)
	{
		queue = new Queue<T>(collection);
	}

	public void Enqueue(T item)
	{
		lock (SyncRoot)
		{
			int count = queue.Count;
			queue.Enqueue(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, count);
				collectionChanged(in e);
			}
		}
	}

	public void EnqueueRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			int count = queue.Count;
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			ReadOnlySpan<T> span = cloneCollection.Span;
			for (int i = 0; i < span.Length; i++)
			{
				T item = span[i];
				queue.Enqueue(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, count);
				collectionChanged(in e);
			}
		}
	}

	public void EnqueueRange(T[] items)
	{
		lock (SyncRoot)
		{
			int count = queue.Count;
			foreach (T item in items)
			{
				queue.Enqueue(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public void EnqueueRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			int count = queue.Count;
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				queue.Enqueue(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, count);
				collectionChanged(in e);
			}
		}
	}

	public T Dequeue()
	{
		lock (SyncRoot)
		{
			T val = queue.Dequeue();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(val, 0);
				collectionChanged(in e);
			}
			return val;
		}
	}

	public bool TryDequeue([MaybeNullWhen(false)] out T result)
	{
		lock (SyncRoot)
		{
			if (queue.Count != 0)
			{
				result = queue.Dequeue();
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(result, 0);
					collectionChanged(in e);
				}
				return true;
			}
			result = default(T);
			return false;
		}
	}

	public void DequeueRange(int count)
	{
		lock (SyncRoot)
		{
			T[] array = ArrayPool<T>.Shared.Rent(count);
			try
			{
				for (int i = 0; i < count; i++)
				{
					array[i] = queue.Dequeue();
				}
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(array.AsSpan(0, count), 0);
					collectionChanged(in e);
				}
			}
			finally
			{
				ArrayPool<T>.Shared.Return(array, RuntimeHelpersEx.IsReferenceOrContainsReferences<T>());
			}
		}
	}

	public void DequeueRange(Span<T> dest)
	{
		lock (SyncRoot)
		{
			for (int i = 0; i < dest.Length; i++)
			{
				dest[i] = queue.Dequeue();
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(dest, 0);
				collectionChanged(in e);
			}
		}
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			queue.Clear();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Reset();
				collectionChanged(in e);
			}
		}
	}

	public T Peek()
	{
		lock (SyncRoot)
		{
			return queue.Peek();
		}
	}

	public bool TryPeek([MaybeNullWhen(false)] T result)
	{
		lock (SyncRoot)
		{
			if (queue.Count != 0)
			{
				result = queue.Peek();
				return true;
			}
			result = default(T);
			return false;
		}
	}

	public T[] ToArray()
	{
		lock (SyncRoot)
		{
			return queue.ToArray();
		}
	}

	public void TrimExcess()
	{
		lock (SyncRoot)
		{
			queue.TrimExcess();
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (T item in queue)
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
