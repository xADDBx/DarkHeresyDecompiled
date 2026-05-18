using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

public static class WidgetListExtensions
{
	public static WidgetListSubscription<TData, TView> SubscribeToWidgetList<TData, TView>(this IEnumerable<TData> data, WidgetList widgetList, TView prefab) where TView : MonoBehaviour, IBindable<TData>
	{
		return new WidgetListSubscription<TData, TView>(widgetList, data, prefab);
	}

	public static WidgetListSubscription<TData, TView> SubscribeToWidgetList<TData, TView>(this IEnumerable<TData> data, WidgetList widgetList, IEnumerable<TView> prefabs) where TView : MonoBehaviour, IBindable<TData>
	{
		return new WidgetListSubscription<TData, TView>(widgetList, data, prefabs);
	}

	public static WidgetListSubscription<TData, MonoBehaviour> SubscribeToWidgetList<TData>(this IEnumerable<TData> data, WidgetList widgetList, IEnumerable<MonoBehaviour> prefabs)
	{
		return new WidgetListSubscription<TData, MonoBehaviour>(widgetList, data, prefabs);
	}
}
