using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal static class InteractionNavigationUtility
{
	public static void Notify(List<Entity> prev, List<Entity> curr, Entity selected)
	{
		foreach (Entity item in prev)
		{
			if (!curr.Contains(item))
			{
				Notify(item, isInNavigation: false, isChosen: false);
			}
		}
		foreach (Entity item2 in curr)
		{
			Notify(item2, curr.Count > 1, item2 == selected);
		}
	}

	private static void Notify(Entity entity, bool isInNavigation, bool isChosen)
	{
		IEntityView view2 = entity.View;
		EntityViewBase view = view2 as EntityViewBase;
		if ((object)view != null)
		{
			if (view is AbstractUnitEntityView abstractUnitEntityView)
			{
				abstractUnitEntityView.MouseHoverHighlighting = isChosen;
			}
			else if (view is MapObjectView mapObjectView)
			{
				mapObjectView.ForceHighlightExternal(isChosen);
			}
			EventBus.RaiseEvent(delegate(ISurroundingInteractableObjectsCountHandler h)
			{
				h.HandleSurroundingInteractableObjectsCountChanged(view, isInNavigation, isChosen);
			});
		}
	}

	public static void GetEntitiesNonAlloc(Vector3 position, float radius, Predicate<Entity> filter, IComparer<Entity> sorter, List<Entity> results)
	{
		foreach (Entity item in EntityBoundsHelper.FindEntitiesInRange(position, radius))
		{
			if (filter(item))
			{
				results.Add(item);
			}
		}
		results.Sort(sorter);
	}

	public static bool AreEqual<T>(IReadOnlyList<T> left, IReadOnlyList<T> right) where T : class
	{
		if (left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	public static int Wrap(int x, int mod)
	{
		return (x % mod + mod) % mod;
	}
}
