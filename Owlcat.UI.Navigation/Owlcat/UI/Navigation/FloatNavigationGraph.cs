using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Owlcat.UI.Navigation;

internal class FloatNavigationGraph : INavigationGraph
{
	private static Selectable[] s_Selectables = new Selectable[128];

	private readonly GameObject m_Root;

	public FloatNavigationGraph(GameObject root)
	{
		m_Root = root;
	}

	public bool TryGet([NotNull] GameObject selected, Vector2 dir, out GameObject result)
	{
		result = FindSelectable(m_Root.transform, selected, dir);
		return result != null;
	}

	private static void GetSelectablesNonAlloc(Transform root, List<RectTransform> result)
	{
		int num;
		while (true)
		{
			num = Selectable.AllSelectablesNoAlloc(s_Selectables);
			if (num != s_Selectables.Length)
			{
				break;
			}
			Array.Resize(ref s_Selectables, s_Selectables.Length * 2);
		}
		for (int i = 0; i < num; i++)
		{
			Selectable selectable = s_Selectables[i];
			if (selectable.isActiveAndEnabled && selectable.IsInteractable() && selectable.navigation.mode != 0 && (!selectable.TryGetComponent<FloatNavigationComponent>(out var component) || !component.Ignore) && selectable.transform.IsChildOf(root))
			{
				result.Add(selectable.transform as RectTransform);
			}
		}
		List<OwlcatSelectable> value;
		using (CollectionPool<List<OwlcatSelectable>, OwlcatSelectable>.Get(out value))
		{
			GetComponentsInChildren(root, value);
			foreach (OwlcatSelectable item in value)
			{
				if (item.isActiveAndEnabled && item.Interactable && (!item.TryGetComponent<FloatNavigationComponent>(out var component2) || !component2.Ignore))
				{
					result.Add(item.transform as RectTransform);
				}
			}
		}
	}

	private static void GetComponentsInChildren<T>(Transform root, List<T> result)
	{
		if (root.TryGetComponent<T>(out var component))
		{
			result.Add(component);
		}
		for (int i = 0; i < root.childCount; i++)
		{
			GetComponentsInChildren(root.GetChild(i), result);
		}
	}

	public static GameObject FindSelectable(Transform root, GameObject selected, Vector3 dir)
	{
		List<RectTransform> value;
		if (selected.TryGetComponent<RectTransform>(out var component))
		{
			using (CollectionPool<List<RectTransform>, RectTransform>.Get(out value))
			{
				GetSelectablesNonAlloc(root, value);
				return FloatNavigationUtility.FindSelectable(component, null, dir, value);
			}
		}
		return FindSelectable(root, selected.transform.position, dir);
	}

	public static GameObject FindSelectable(Transform root, Vector2 pos, Vector3 dir)
	{
		List<RectTransform> value;
		using (CollectionPool<List<RectTransform>, RectTransform>.Get(out value))
		{
			GetSelectablesNonAlloc(root, value);
			return FloatNavigationUtility.FindSelectable(null, pos, dir, value);
		}
	}
}
