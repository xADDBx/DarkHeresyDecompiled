using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableCollections.Internal;

internal class NotifyCollectionChangedSynchronizedView<T, TView> : INotifyCollectionChangedSynchronizedView<TView>, IReadOnlyCollection<TView>, IEnumerable<TView>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable, ISynchronizedViewFilter<T, TView>
{
	private static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new PropertyChangedEventArgs("Count");

	private readonly ISynchronizedView<T, TView> parent;

	private readonly ISynchronizedViewFilter<T, TView> currentFilter;

	public int Count => parent.Count;

	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	public event PropertyChangedEventHandler? PropertyChanged;

	public event Action<NotifyCollectionChangedAction>? CollectionStateChanged
	{
		add
		{
			parent.CollectionStateChanged += value;
		}
		remove
		{
			parent.CollectionStateChanged -= value;
		}
	}

	public event NotifyCollectionChangedEventHandler<T>? RoutingCollectionChanged
	{
		add
		{
			parent.RoutingCollectionChanged += value;
		}
		remove
		{
			parent.RoutingCollectionChanged -= value;
		}
	}

	public NotifyCollectionChangedSynchronizedView(ISynchronizedView<T, TView> parent)
	{
		this.parent = parent;
		currentFilter = parent.CurrentFilter;
		parent.AttachFilter(this);
	}

	public void Dispose()
	{
		parent.Dispose();
	}

	public IEnumerator<TView> GetEnumerator()
	{
		foreach (var item in parent)
		{
			yield return item.View;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool IsMatch(T value, TView view)
	{
		return currentFilter.IsMatch(value, view);
	}

	public void WhenTrue(T value, TView view)
	{
		currentFilter.WhenTrue(value, view);
	}

	public void WhenFalse(T value, TView view)
	{
		currentFilter.WhenFalse(value, view);
	}

	public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> args)
	{
		currentFilter.OnCollectionChanged(in args);
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewView, args.NewViewIndex));
			this.PropertyChanged?.Invoke(this, CountPropertyChangedEventArgs);
			break;
		case NotifyCollectionChangedAction.Remove:
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldView, args.OldViewIndex));
			this.PropertyChanged?.Invoke(this, CountPropertyChangedEventArgs);
			break;
		case NotifyCollectionChangedAction.Reset:
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			this.PropertyChanged?.Invoke(this, CountPropertyChangedEventArgs);
			break;
		case NotifyCollectionChangedAction.Replace:
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewView, args.OldView, args.NewViewIndex));
			break;
		case NotifyCollectionChangedAction.Move:
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.NewView, args.NewViewIndex, args.OldViewIndex));
			break;
		}
	}

	void ISynchronizedViewFilter<T, TView>.OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
	{
		OnCollectionChanged(in eventArgs);
	}
}
