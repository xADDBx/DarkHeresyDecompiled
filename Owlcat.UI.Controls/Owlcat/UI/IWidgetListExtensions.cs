using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using UnityEngine;

namespace Owlcat.UI;

[Obsolete("Используйте привязки к коллекциям через .SubscribeToWidgetList() метод-расширение")]
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

	[Obsolete]
	public static IDisposable SubscribeToWidgetList<TData, TView>(this IObservableCollection<TData> data, WidgetList widgetList, TView prefab, Predicate<TData> filter) where TView : MonoBehaviour, IBindable<TData>
	{
		return data.SubscribeToWidgetList(widgetList, prefab, filter);
	}
}
