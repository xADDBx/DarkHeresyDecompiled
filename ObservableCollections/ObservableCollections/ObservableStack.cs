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

public sealed class ObservableStack<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IObservableCollection<T>
{
	private class View<TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly ObservableStack<T> source;

		private readonly Func<T, TView> selector;

		private readonly bool reverse;

		protected readonly Stack<(T, TView)> stack;

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
					return stack.Count;
				}
			}
		}

		public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

		public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

		public View(ObservableStack<T> source, Func<T, TView> selector, bool reverse)
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
				stack = new Stack<(T, TView)>(source.stack.Select<T, (T, TView)>((T x) => (x: x, selector(x))));
				this.source.CollectionChanged += SourceCollectionChanged;
			}
		}

		public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
		{
			lock (SyncRoot)
			{
				this.filter = filter;
				foreach (var (value, view) in stack)
				{
					if (invokeAddEventForCurrentElements)
					{
						filter.InvokeOnAdd(value, view, 0);
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
				foreach (var (arg, arg2) in stack)
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
					foreach (var item in stack)
					{
						if (filter.IsMatch(item.Item1, item.Item2))
						{
							yield return item;
						}
					}
					yield break;
				}
				foreach (var item2 in stack.AsEnumerable().Reverse())
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
						stack.Push(tuple3);
						filter.InvokeOnAdd(tuple3, 0);
						break;
					}
					ReadOnlySpan<T> newItems = e.NewItems;
					for (int j = 0; j < newItems.Length; j++)
					{
						T val = newItems[j];
						(T, TView) tuple4 = (val, selector(val));
						stack.Push(tuple4);
						filter.InvokeOnAdd(tuple4, 0);
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					if (e.IsSingleItem)
					{
						(T, TView) tuple = stack.Pop();
						filter.InvokeOnRemove(tuple.Item1, tuple.Item2, 0);
						break;
					}
					int length = e.OldItems.Length;
					for (int i = 0; i < length; i++)
					{
						(T, TView) tuple2 = stack.Pop();
						filter.InvokeOnRemove(tuple2.Item1, tuple2.Item2, 0);
					}
					break;
				}
				case NotifyCollectionChangedAction.Reset:
					stack.Clear();
					filter.InvokeOnReset();
					break;
				}
				this.RoutingCollectionChanged?.Invoke(in e);
				this.CollectionStateChanged?.Invoke(e.Action);
			}
		}
	}

	private readonly Stack<T> stack;

	public object SyncRoot { get; } = new object();


	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return stack.Count;
			}
		}
	}

	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	public ObservableStack()
	{
		stack = new Stack<T>();
	}

	public ObservableStack(int capacity)
	{
		stack = new Stack<T>(capacity);
	}

	public ObservableStack(IEnumerable<T> collection)
	{
		stack = new Stack<T>(collection);
	}

	public void Push(T item)
	{
		lock (SyncRoot)
		{
			stack.Push(item);
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, 0);
				collectionChanged(in e);
			}
		}
	}

	public void PushRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			using CloneCollection<T> cloneCollection = new CloneCollection<T>(items);
			ReadOnlySpan<T> span = cloneCollection.Span;
			for (int i = 0; i < span.Length; i++)
			{
				T item = span[i];
				stack.Push(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(cloneCollection.Span, 0);
				collectionChanged(in e);
			}
		}
	}

	public void PushRange(T[] items)
	{
		lock (SyncRoot)
		{
			foreach (T item in items)
			{
				stack.Push(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, 0);
				collectionChanged(in e);
			}
		}
	}

	public void PushRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				stack.Push(item);
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(items, 0);
				collectionChanged(in e);
			}
		}
	}

	public T Pop()
	{
		lock (SyncRoot)
		{
			T val = stack.Pop();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(val, 0);
				collectionChanged(in e);
			}
			return val;
		}
	}

	public bool TryPop([MaybeNullWhen(false)] out T result)
	{
		lock (SyncRoot)
		{
			if (stack.Count != 0)
			{
				result = stack.Pop();
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

	public void PopRange(int count)
	{
		lock (SyncRoot)
		{
			T[] array = ArrayPool<T>.Shared.Rent(count);
			try
			{
				for (int i = 0; i < count; i++)
				{
					array[i] = stack.Pop();
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

	public void PopRange(Span<T> dest)
	{
		lock (SyncRoot)
		{
			for (int i = 0; i < dest.Length; i++)
			{
				dest[i] = stack.Pop();
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
			stack.Clear();
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
			return stack.Peek();
		}
	}

	public bool TryPeek([MaybeNullWhen(false)] T result)
	{
		lock (SyncRoot)
		{
			if (stack.Count != 0)
			{
				result = stack.Peek();
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
			return stack.ToArray();
		}
	}

	public void TrimExcess()
	{
		lock (SyncRoot)
		{
			stack.TrimExcess();
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (T item in stack)
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
