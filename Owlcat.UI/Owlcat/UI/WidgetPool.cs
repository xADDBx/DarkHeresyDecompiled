using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.UI;

public static class WidgetPool
{
	private static readonly Dictionary<int, IList> sWidgets = new Dictionary<int, IList>();

	private static readonly Dictionary<int, int> sWidgetMap = new Dictionary<int, int>();

	private static GameObject sStash;

	private static WidgetPoolSettings sSettings;

	public static void Clear()
	{
		sWidgets.Clear();
		sWidgetMap.Clear();
		if (sStash != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(sStash);
			}
			else
			{
				Object.DestroyImmediate(sStash);
			}
			sStash = null;
		}
	}

	public static void Initialize(WidgetPoolSettings settings)
	{
		sSettings = settings;
	}

	public static T Retain<T>(T prefab, Transform parent) where T : Component
	{
		if (!TryPool(prefab, out var widget))
		{
			widget = Object.Instantiate(prefab, null);
			sWidgetMap[widget.transform.GetInstanceID()] = prefab.transform.GetInstanceID();
		}
		widget.transform.SetParent(parent, worldPositionStays: false);
		widget.transform.SetTransform(prefab.transform);
		widget.gameObject.SetActive(value: true);
		return widget;
	}

	public static async Task<T> RetainAsync<T>(T prefab, Transform parent, CancellationToken cancellationToken) where T : Component
	{
		if (TryPool(prefab, out var widget))
		{
			widget.transform.SetParent(parent, worldPositionStays: false);
			widget.transform.SetTransform(prefab.transform);
			widget.gameObject.SetActive(value: true);
			return widget;
		}
		int prefabId = prefab.transform.GetInstanceID();
		T[] array = await Object.InstantiateAsync(prefab, 1, null, Vector3.zero, Quaternion.identity, cancellationToken);
		T[] array2 = array;
		foreach (T val in array2)
		{
			sWidgetMap[val.transform.GetInstanceID()] = prefabId;
		}
		if ((parent == null && (object)parent != null) || cancellationToken.IsCancellationRequested || !Application.isPlaying)
		{
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Release(array2[i]);
			}
			throw new TaskCanceledException();
		}
		array2 = array;
		foreach (T val2 in array2)
		{
			val2.transform.SetParent(parent, worldPositionStays: false);
			val2.transform.SetTransform(prefab.transform);
			val2.gameObject.SetActive(value: true);
		}
		return array[0];
	}

	private static bool TryPool<T>(T prefab, out T widget) where T : Component
	{
		if (sWidgets.TryGetValue(prefab.transform.GetInstanceID(), out var value))
		{
			while (value.Count > 0 && value[0].Equals(null))
			{
				value.RemoveAt(0);
			}
			if (value.Count > 0)
			{
				widget = (T)value[0];
				value.RemoveAt(0);
				return true;
			}
		}
		widget = null;
		return false;
	}

	private static void SetTransform(this Transform transform, Transform prefab)
	{
		if (transform is RectTransform rectTransform && prefab is RectTransform rectTransform2)
		{
			rectTransform.anchoredPosition3D = rectTransform2.anchoredPosition3D;
		}
		else
		{
			transform.SetLocalPositionAndRotation(prefab.position, prefab.rotation);
		}
	}

	public static void Release<T>(T widget, bool reparent = true) where T : Component
	{
		if (widget == null)
		{
			return;
		}
		if (sWidgetMap.TryGetValue(widget.transform.GetInstanceID(), out var value))
		{
			if (!sWidgets.TryGetValue(value, out var value2))
			{
				value2 = (sWidgets[value] = new List<Component>());
			}
			if (widget is IBindable bindable)
			{
				bindable.Unbind();
			}
			widget.gameObject.SetActive(value: false);
			value2.Add(widget);
			if (!reparent)
			{
				return;
			}
			if (sStash == null)
			{
				sStash = new GameObject("[WidgetPool]");
				sStash.SetActive(value: false);
				sStash.hideFlags = sSettings.HideFlags;
				if (sSettings.DontDestroyOnLoad)
				{
					Object.DontDestroyOnLoad(sStash);
				}
				else if (sSettings.Scene.IsValid())
				{
					SceneManager.MoveGameObjectToScene(sStash, sSettings.Scene);
				}
			}
			widget.transform.SetParent(sStash.transform, worldPositionStays: false);
		}
		else
		{
			Object.Destroy(widget.gameObject);
		}
	}
}
