using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.UI;

public class WidgetListSubscription<TData, TView> : IDisposable, IUIJob where TView : MonoBehaviour
{
	private readonly WidgetList m_WidgetList;

	private readonly IEnumerable<TData> m_Data;

	private readonly IEnumerable<TView> m_View;

	private Predicate<TData> m_Filter;

	private Comparison<TData> m_Comparison;

	private Action<TView> m_Initializer;

	private Action<List<TData>> m_OnBeforeDraw;

	private Action m_OnAfterDraw;

	private DisposableBag m_Disposables;

	public WidgetListSubscription(WidgetList widgetList, IEnumerable<TData> data, TView view)
		: this(widgetList, data, (IEnumerable<TView>)new TView[1] { view })
	{
	}

	public WidgetListSubscription(WidgetList widgetList, IEnumerable<TData> data, IEnumerable<TView> view)
	{
		m_WidgetList = widgetList;
		m_Data = data;
		m_View = view;
		Initialize();
	}

	public WidgetListSubscription<TData, TView> WithFilter(Predicate<TData> filter)
	{
		m_Filter = filter;
		return this;
	}

	public WidgetListSubscription<TData, TView> WithSorter(IComparer<TData> comparer)
	{
		return WithSorter((comparer != null) ? new Comparison<TData>(comparer.Compare) : null);
	}

	public WidgetListSubscription<TData, TView> WithSorter(Comparison<TData> comparison)
	{
		m_Comparison = comparison;
		return this;
	}

	public WidgetListSubscription<TData, TView> WithInitializer(Action<TView> initializer)
	{
		m_Initializer = initializer;
		return this;
	}

	public WidgetListSubscription<TData, TView> WithOnBeforeDraw(Action<List<TData>> callback)
	{
		m_OnBeforeDraw = callback;
		return this;
	}

	public WidgetListSubscription<TData, TView> WithOnAfterDraw(Action callback)
	{
		m_OnAfterDraw = callback;
		return this;
	}

	private void Initialize()
	{
		Disposable.Create(m_WidgetList.Clear).AddTo(ref m_Disposables);
		IEnumerable<TData> data = m_Data;
		IObservableCollection<TData> observable = data as IObservableCollection<TData>;
		if (observable != null)
		{
			Observable.FromEvent(Convert<TData>, delegate(NotifyCollectionChangedEventHandler<TData> h)
			{
				observable.CollectionChanged += h;
			}, delegate(NotifyCollectionChangedEventHandler<TData> h)
			{
				observable.CollectionChanged -= h;
			}).Prepend(Unit.Default).Subscribe(Enqueue)
				.AddTo(ref m_Disposables);
		}
		else
		{
			Enqueue();
		}
		static NotifyCollectionChangedEventHandler<U> Convert<U>(Action h)
		{
			return delegate
			{
				h();
			};
		}
	}

	private void Enqueue()
	{
		IUIJob.Enqueue(this);
	}

	void IUIJob.Run()
	{
		List<TData> value;
		using (CollectionPool<List<TData>, TData>.Get(out value))
		{
			foreach (TData datum in m_Data)
			{
				if (m_Filter == null || m_Filter(datum))
				{
					value.Add(datum);
				}
			}
			if (m_Comparison != null)
			{
				value.Sort(m_Comparison);
			}
			if (m_Initializer != null)
			{
				m_WidgetList.Clear();
			}
			m_OnBeforeDraw?.Invoke(value);
			m_WidgetList.DrawMultiEntries((IEnumerable<TData>)value, (IEnumerable<MonoBehaviour>)m_View);
			if (m_Initializer != null)
			{
				foreach (IBindable entry in m_WidgetList.Entries)
				{
					m_Initializer((TView)entry);
				}
			}
			m_OnAfterDraw?.Invoke();
		}
	}

	public void Add(IDisposable disposable)
	{
		m_Disposables.Add(disposable);
	}

	public void Dispose()
	{
		IUIJob.Remove(this);
		m_Disposables.Dispose();
	}
}
