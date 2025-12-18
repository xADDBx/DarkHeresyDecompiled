using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.UI;

[Obsolete]
public static class IWidgetListExtensions
{
	public static IDisposable DrawMultiEntries<TWidget, TData>(this WidgetList list, IEnumerable<TData> vmCollection, List<TWidget> entryPrefabs) where TWidget : IWidgetView
	{
		return list.DrawMultiEntries(vmCollection, entryPrefabs.Cast<MonoBehaviour>());
	}

	public static IDisposable DrawEntries<TWidget, TData>(this WidgetList list, IEnumerable<TData> vmCollection, TWidget entryPrefab) where TWidget : IWidgetView
	{
		return list.DrawEntries(vmCollection, entryPrefab as View<TData>);
	}

	public static IDisposable SubscribeToWidgetList<TData, TView>(this IObservableCollection<TData> data, WidgetList widgetList, TView prefab, Predicate<TData> filter = null) where TView : MonoBehaviour, IBindable<TData>
	{
		return new CompositeDisposable
		{
			Observable.FromEvent(Convert<TData>, delegate(NotifyCollectionChangedEventHandler<TData> h)
			{
				data.CollectionChanged += h;
			}, delegate(NotifyCollectionChangedEventHandler<TData> h)
			{
				data.CollectionChanged -= h;
			}).Prepend(Unit.Default).DebounceFrame(1)
				.Subscribe(Draw),
			Disposable.Create(widgetList.Clear)
		};
		void Draw()
		{
			List<TData> value;
			using (CollectionPool<List<TData>, TData>.Get(out value))
			{
				foreach (TData datum in data)
				{
					if (filter == null || filter(datum))
					{
						value.Add(datum);
					}
				}
				widgetList.DrawEntries(value, prefab);
			}
		}
	}

	private static NotifyCollectionChangedEventHandler<TData> Convert<TData>(Action h)
	{
		return delegate
		{
			h();
		};
	}
}
