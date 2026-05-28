using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Owlcat.UI.Navigation;

internal static class FocusUtility
{
	public static int GetHierarhyNonAlloc<T>(GameObject root, GameObject leaf, ICollection<T> result)
	{
		List<T> value;
		using (CollectionPool<List<T>, T>.Get(out value))
		{
			Transform transform = leaf.transform;
			while (transform != null && transform.IsChildOf(root.transform))
			{
				if (transform.TryGetComponent<T>(out var component) && !value.Contains(component))
				{
					value.Add(component);
				}
				transform = transform.parent;
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				result.Add(value[num]);
			}
			return value.Count;
		}
	}

	public static bool TrySelect<T>(this EventSystem system, GameObject root, GameObject leaf, Predicate<T> filter) where T : Component
	{
		List<T> value;
		using (CollectionPool<List<T>, T>.Get(out value))
		{
			GetHierarhyNonAlloc(root, leaf, value);
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (filter(value[num]))
				{
					system.SetSelectedGameObject(value[num].gameObject);
					return true;
				}
			}
			return false;
		}
	}
}
