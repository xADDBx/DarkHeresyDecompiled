using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ObservableCollections.Internal;

namespace ObservableCollections;

public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyObservableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IObservableCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
{
	private class View<TView> : ISynchronizedView<KeyValuePair<TKey, TValue>, TView>, IReadOnlyCollection<(KeyValuePair<TKey, TValue> Value, TView View)>, IEnumerable<(KeyValuePair<TKey, TValue> Value, TView View)>, IEnumerable, IDisposable
	{
		private readonly ObservableDictionary<TKey, TValue> source;

		private readonly Func<KeyValuePair<TKey, TValue>, TView> selector;

		private ISynchronizedViewFilter<KeyValuePair<TKey, TValue>, TView> filter;

		private readonly Dictionary<TKey, (TValue, TView)> dict;

		public object SyncRoot { get; }

		public ISynchronizedViewFilter<KeyValuePair<TKey, TValue>, TView> CurrentFilter
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
					return dict.Count;
				}
			}
		}

		public event NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? RoutingCollectionChanged;

		public event Action<NotifyCollectionChangedAction>? CollectionStateChanged;

		public View(ObservableDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, TView> selector)
		{
			Func<KeyValuePair<TKey, TValue>, TView> selector = selector;
			base._002Ector();
			this.source = source;
			this.selector = selector;
			filter = SynchronizedViewFilter<KeyValuePair<TKey, TValue>, TView>.Null;
			SyncRoot = new object();
			lock (source.SyncRoot)
			{
				dict = source.dictionary.ToDictionary<KeyValuePair<TKey, TValue>, TKey, (TValue, TView)>((KeyValuePair<TKey, TValue> x) => x.Key, (KeyValuePair<TKey, TValue> x) => (Value: x.Value, selector(x)));
				this.source.CollectionChanged += SourceCollectionChanged;
			}
		}

		public void Dispose()
		{
			source.CollectionChanged -= SourceCollectionChanged;
		}

		public void AttachFilter(ISynchronizedViewFilter<KeyValuePair<TKey, TValue>, TView> filter, bool invokeAddEventForCurrentElements = false)
		{
			lock (SyncRoot)
			{
				this.filter = filter;
				foreach (KeyValuePair<TKey, (TValue, TView)> item2 in dict)
				{
					KeyValuePair<TKey, TValue> value = new KeyValuePair<TKey, TValue>(item2.Key, item2.Value.Item1);
					TView item = item2.Value.Item2;
					if (invokeAddEventForCurrentElements)
					{
						filter.InvokeOnAdd(value, item, -1);
					}
					else
					{
						filter.InvokeOnAttach(value, item);
					}
				}
			}
		}

		public void ResetFilter(Action<KeyValuePair<TKey, TValue>, TView>? resetAction)
		{
			lock (SyncRoot)
			{
				filter = SynchronizedViewFilter<KeyValuePair<TKey, TValue>, TView>.Null;
				if (resetAction == null)
				{
					return;
				}
				foreach (KeyValuePair<TKey, (TValue, TView)> item in dict)
				{
					resetAction(new KeyValuePair<TKey, TValue>(item.Key, item.Value.Item1), item.Value.Item2);
				}
			}
		}

		public INotifyCollectionChangedSynchronizedView<TView> ToNotifyCollectionChanged()
		{
			lock (SyncRoot)
			{
				return new NotifyCollectionChangedSynchronizedView<KeyValuePair<TKey, TValue>, TView>(this);
			}
		}

		public IEnumerator<(KeyValuePair<TKey, TValue>, TView)> GetEnumerator()
		{
			lock (SyncRoot)
			{
				foreach (KeyValuePair<TKey, (TValue, TView)> item in dict)
				{
					(KeyValuePair<TKey, TValue>, TView) tuple = (new KeyValuePair<TKey, TValue>(item.Key, item.Value.Item1), item.Value.Item2);
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

		private void SourceCollectionChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e)
		{
			lock (SyncRoot)
			{
				switch (e.Action)
				{
				case NotifyCollectionChangedAction.Add:
				{
					TView val2 = selector(e.NewItem);
					dict.Add(e.NewItem.Key, (e.NewItem.Value, val2));
					filter.InvokeOnAdd(e.NewItem, val2, -1);
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					if (dict.Remove(e.OldItem.Key, out var value2))
					{
						filter.InvokeOnRemove(e.OldItem, value2.Item2, -1);
					}
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				{
					TView val = selector(e.NewItem);
					dict.Remove(e.OldItem.Key, out var value);
					dict[e.NewItem.Key] = (e.NewItem.Value, val);
					filter.InvokeOnReplace(e.NewItem, val, e.OldItem, value.Item2, -1);
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

	private readonly Dictionary<TKey, TValue> dictionary;

	public object SyncRoot { get; } = new object();


	public TValue this[TKey key]
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary[key];
			}
		}
		set
		{
			lock (SyncRoot)
			{
				if (dictionary.TryGetValue(key, out var value2))
				{
					dictionary[key] = value;
					NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? collectionChanged = this.CollectionChanged;
					if (collectionChanged != null)
					{
						NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Replace(new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, value2), -1, -1);
						collectionChanged(in e);
					}
				}
				else
				{
					Add(key, value);
				}
			}
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary.Keys;
			}
		}
	}

	ICollection<TValue> IDictionary<TKey, TValue>.Values
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary.Values;
			}
		}
	}

	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary.Count;
			}
		}
	}

	public bool IsReadOnly => false;

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary.Keys;
			}
		}
	}

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
	{
		get
		{
			lock (SyncRoot)
			{
				return dictionary.Values;
			}
		}
	}

	public event NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? CollectionChanged;

	public ObservableDictionary()
	{
		dictionary = new Dictionary<TKey, TValue>();
	}

	public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
	{
		dictionary = new Dictionary<TKey, TValue>();
		foreach (KeyValuePair<TKey, TValue> item in collection)
		{
			dictionary.Add(item.Key, item.Value);
		}
	}

	public void Add(TKey key, TValue value)
	{
		lock (SyncRoot)
		{
			dictionary.Add(key, value);
			NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Add(new KeyValuePair<TKey, TValue>(key, value), -1);
				collectionChanged(in e);
			}
		}
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			dictionary.Clear();
			NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Reset();
				collectionChanged(in e);
			}
		}
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		lock (SyncRoot)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
		}
	}

	public bool ContainsKey(TKey key)
	{
		lock (SyncRoot)
		{
			return ((IDictionary<TKey, TValue>)dictionary).ContainsKey(key);
		}
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		lock (SyncRoot)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
		}
	}

	public bool Remove(TKey key)
	{
		lock (SyncRoot)
		{
			if (dictionary.Remove(key, out var value))
			{
				NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(new KeyValuePair<TKey, TValue>(key, value), -1);
					collectionChanged(in e);
				}
				return true;
			}
			return false;
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		lock (SyncRoot)
		{
			if (dictionary.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value) && dictionary.Remove(item.Key, out var value2))
			{
				NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? collectionChanged = this.CollectionChanged;
				if (collectionChanged != null)
				{
					NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> e = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(new KeyValuePair<TKey, TValue>(item.Key, value2), -1);
					collectionChanged(in e);
				}
				return true;
			}
			return false;
		}
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		lock (SyncRoot)
		{
			return dictionary.TryGetValue(key, out value);
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ISynchronizedView<KeyValuePair<TKey, TValue>, TView> CreateView<TView>(Func<KeyValuePair<TKey, TValue>, TView> transform, bool _ = false)
	{
		return new View<TView>(this, transform);
	}
}
