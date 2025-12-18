using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableHashSet<T> : IReadOnlySet<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, IObservableCollection<T> where T : notnull
{
	private sealed class View<TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly ObservableHashSet<T> source;

		private readonly Func<T, TView> selector;

		private readonly Dictionary<T, (T, TView)> dict;

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
					return dict.Count;
				}
			}
		}

		public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged;

		public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

		public View(ObservableHashSet<T> source, Func<T, TView> selector)
		{
			Func<T, TView> selector = selector;
			base._002Ector();
			this.source = source;
			this.selector = selector;
			filter = SynchronizedViewFilter<T, TView>.Null;
			SyncRoot = new object();
			lock (source.SyncRoot)
			{
				dict = source.set.ToDictionary<T, T, (T, TView)>((T x) => x, (T x) => (x: x, selector(x)));
				this.source.CollectionChanged += SourceCollectionChanged;
			}
		}

		public void AttachFilter(ISynchronizedViewFilter<T, TView> filter, bool invokeAddEventForCurrentElements = false)
		{
			lock (SyncRoot)
			{
				this.filter = filter;
				foreach (KeyValuePair<T, (T, TView)> item in dict)
				{
					item.Deconstruct(out var _, out var value);
					var (val, val2) = value;
					if (invokeAddEventForCurrentElements)
					{
						filter.InvokeOnAdd((value: val, view: val2), -1);
					}
					else
					{
						filter.InvokeOnAttach(val, val2);
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
				foreach (KeyValuePair<T, (T, TView)> item in dict)
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
				foreach (KeyValuePair<T, (T, TView)> item in dict)
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
						(T, TView) value3 = (e.NewItem, selector(e.NewItem));
						dict.Add(e.NewItem, value3);
						filter.InvokeOnAdd(value3, -1);
						break;
					}
					int newStartingIndex = e.NewStartingIndex;
					ReadOnlySpan<T> oldItems = e.NewItems;
					for (int i = 0; i < oldItems.Length; i++)
					{
						T val = oldItems[i];
						(T, TView) value4 = (val, selector(val));
						dict.Add(val, value4);
						filter.InvokeOnAdd(value4, newStartingIndex++);
					}
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					if (e.IsSingleItem)
					{
						if (dict.Remove(e.OldItem, out var value))
						{
							filter.InvokeOnRemove(value, -1);
						}
						break;
					}
					ReadOnlySpan<T> oldItems = e.OldItems;
					for (int i = 0; i < oldItems.Length; i++)
					{
						T key = oldItems[i];
						if (dict.Remove(key, out var value2))
						{
							filter.InvokeOnRemove(value2, -1);
						}
					}
					break;
				}
				case NotifyCollectionChangedAction.Reset:
					dict.Clear();
					filter.InvokeOnReset();
					break;
				}
				this.RoutingCollectionChanged?.Invoke(in e);
				this.CollectionStateChanged?.Invoke(e.Action);
			}
		}
	}

	private readonly HashSet<T> set;

	public object SyncRoot { get; } = new object();


	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return set.Count;
			}
		}
	}

	public bool IsReadOnly => false;

	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;

	public ObservableHashSet()
	{
		set = new HashSet<T>();
	}

	public ObservableHashSet(int capacity)
	{
		set = new HashSet<T>(capacity);
	}

	public ObservableHashSet(IEnumerable<T> collection)
	{
		set = new HashSet<T>(collection);
	}

	public bool Add(T item)
	{
		lock (SyncRoot)
		{
			if (set.Add(item))
			{
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(item, -1);
					collectionChanged(in e);
				}
				return true;
			}
			return false;
		}
	}

	public void AddRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			if (!items.TryGetNonEnumeratedCount(out var count))
			{
				count = 4;
			}
			using ResizableArray<T> resizableArray = new ResizableArray<T>(count);
			foreach (T item in items)
			{
				if (set.Add(item))
				{
					resizableArray.Add(item);
				}
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(resizableArray.Span, -1);
				collectionChanged(in e);
			}
		}
	}

	public void AddRange(T[] items)
	{
		AddRange(items.AsSpan());
	}

	public void AddRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			using ResizableArray<T> resizableArray = new ResizableArray<T>(items.Length);
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				if (set.Add(item))
				{
					resizableArray.Add(item);
				}
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Add(resizableArray.Span, -1);
				collectionChanged(in e);
			}
		}
	}

	public bool Remove(T item)
	{
		lock (SyncRoot)
		{
			if (set.Remove(item))
			{
				NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(item, -1);
					collectionChanged(in e);
				}
				return true;
			}
			return false;
		}
	}

	public void RemoveRange(IEnumerable<T> items)
	{
		lock (SyncRoot)
		{
			if (!items.TryGetNonEnumeratedCount(out var count))
			{
				count = 4;
			}
			using ResizableArray<T> resizableArray = new ResizableArray<T>(count);
			foreach (T item in items)
			{
				if (set.Remove(item))
				{
					resizableArray.Add(item);
				}
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(resizableArray.Span, -1);
				collectionChanged(in e);
			}
		}
	}

	public void RemoveRange(T[] items)
	{
		RemoveRange(items.AsSpan());
	}

	public void RemoveRange(ReadOnlySpan<T> items)
	{
		lock (SyncRoot)
		{
			using ResizableArray<T> resizableArray = new ResizableArray<T>(items.Length);
			ReadOnlySpan<T> readOnlySpan = items;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				T item = readOnlySpan[i];
				if (set.Remove(item))
				{
					resizableArray.Add(item);
				}
			}
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Remove(resizableArray.Span, -1);
				collectionChanged(in e);
			}
		}
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			set.Clear();
			NotifyCollectionChangedEventHandler<T>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<T> e = NotifyCollectionChangedEventArgs<T>.Reset();
				collectionChanged(in e);
			}
		}
	}

	public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
	{
		lock (SyncRoot)
		{
			return set.TryGetValue(equalValue, out actualValue);
		}
	}

	public bool Contains(T item)
	{
		lock (SyncRoot)
		{
			return set.Contains(item);
		}
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.IsProperSubsetOf(other);
		}
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.IsProperSupersetOf(other);
		}
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.IsSubsetOf(other);
		}
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.IsSupersetOf(other);
		}
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.Overlaps(other);
		}
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		lock (SyncRoot)
		{
			return set.SetEquals(other);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (T item in set)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform, bool _ = false)
	{
		return new View<TView>(this, transform);
	}
}
