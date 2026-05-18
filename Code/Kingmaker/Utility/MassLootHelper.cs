using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Utility;

public static class MassLootHelper
{
	private class LootDuplicateCheck : IEqualityComparer<InteractionLootPart>
	{
		public bool Equals(InteractionLootPart x, InteractionLootPart y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return x.Loot == y.Loot;
		}

		public int GetHashCode(InteractionLootPart obj)
		{
			return obj.Loot.GetHashCode();
		}
	}

	private const float MaxDistance = 6f;

	private static readonly List<EntityViewBase> Highlighted = new List<EntityViewBase>();

	private static bool m_Cheking;

	public static IEnumerable<EntityViewBase> GetObjectsWithLoot(EntityViewBase src)
	{
		List<EntityViewBase> list = new List<EntityViewBase> { src };
		int num = 0;
		do
		{
			num = CollectObjectsForHighlight(list, num);
		}
		while (num != list.Count);
		return list;
	}

	public static void HighlightLoot(EntityViewBase src, bool active)
	{
		if (!m_Cheking)
		{
			m_Cheking = true;
			if (active)
			{
				DisableHighlight();
				CollectHiglighted(src);
				SetHiglight(value: true);
			}
			else
			{
				DisableHighlight();
			}
			m_Cheking = false;
		}
	}

	public static void Clear()
	{
		Highlighted.Clear();
	}

	private static void DisableHighlight()
	{
		if (Highlighted.Count > 0)
		{
			SetHiglight(value: false);
			Highlighted.Clear();
		}
	}

	private static void CollectHiglighted(EntityViewBase src)
	{
		Highlighted.Add(src);
		int num = 0;
		do
		{
			num = CollectObjectsForHighlight(Highlighted, num);
		}
		while (num != Highlighted.Count);
	}

	private static void SetHiglight(bool value)
	{
		for (int i = 0; i < Highlighted.Count; i++)
		{
			_ = (bool)(Highlighted[i] as UnitEntityView);
			DroppedLootView droppedLootView = Highlighted[i] as DroppedLootView;
			if ((bool)droppedLootView)
			{
				droppedLootView.ForceHighlightExternal(value);
			}
		}
	}

	private static int CollectObjectsForHighlight(List<EntityViewBase> objects, int startIndex)
	{
		int count = objects.Count;
		for (int i = startIndex; i < count; i++)
		{
			EntityViewBase entityViewBase = objects[i];
			UnitEntityView unitEntityView = entityViewBase as UnitEntityView;
			if ((!(unitEntityView != null) || !unitEntityView.EntityData.IsDeadAndHasLoot) && (!(entityViewBase is DroppedLootView) || (bool)entityViewBase.Data.ToEntity().GetOptional<EntityPartBreathOfMoney>()))
			{
				continue;
			}
			foreach (AbstractUnitEntity item in Game.Instance.EntityPools.AllUnits.Where((AbstractUnitEntity uu) => uu.IsDeadAndHasLoot))
			{
				if (item.IsDeadAndHasLoot && !objects.HasItem(item.View.AsEntityView()) && !((entityViewBase.transform.position - item.ViewPosition).To2D().sqrMagnitude > 36f))
				{
					objects.Add(item.View.AsEntityView());
				}
			}
			foreach (MapObjectEntity item2 in Game.Instance.EntityPools.MapObjects.Where((MapObjectEntity mo) => mo.View is DroppedLootView))
			{
				DroppedLootView droppedLootView = item2.View as DroppedLootView;
				if (!(droppedLootView == null) && droppedLootView.Loot.HasLoot && !((MapObjectView)droppedLootView).Data.GetOptional<EntityPartBreathOfMoney>() && !objects.HasItem(droppedLootView) && !((entityViewBase.transform.position - droppedLootView.transform.position).To2D().sqrMagnitude > 36f))
				{
					objects.Add(droppedLootView);
				}
			}
		}
		return count;
	}

	public static bool CanLootZone()
	{
		try
		{
			return GetMassLootFromCurrentArea()?.Any() ?? false;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	public static IEnumerable<LootWrapper> GetMassLootFromCurrentArea()
	{
		List<LootWrapper> list = new List<LootWrapper>();
		foreach (BaseUnitEntity item in Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity u) => u.IsRevealed && u.IsDeadAndHasLoot))
		{
			list.Add(new LootWrapper
			{
				Unit = item
			});
		}
		IEnumerable<InteractionLootPart> enumerable = (from i in Game.Instance.EntityPools.MapObjects
			select i.GetOptional<InteractionLootPart>() into l
			where l == null || l.Settings.LootContainerType != LootContainerType.OneSlot
			select l).Concat(Game.Instance.EntityPools.AllUnits.Select((AbstractUnitEntity i) => i.GetOptional<InteractionLootPart>())).NotNull();
		List<InteractionLootPart> list2 = TempList.Get<InteractionLootPart>();
		foreach (InteractionLootPart item2 in enumerable)
		{
			if (item2.Owner.IsRevealed && item2.Loot.HasLoot && item2.Settings.LootContainerType != LootContainerType.PlayerChest && (item2.LootViewed || (item2.View is DroppedLootView && !item2.Owner.GetOptional<EntityPartBreathOfMoney>()) || (bool)item2.View.GetComponent<SkinnedMeshRenderer>()))
			{
				list2.Add(item2);
			}
		}
		IEnumerable<LootWrapper> collection = list2.Distinct(new LootDuplicateCheck()).Select(delegate(InteractionLootPart i)
		{
			LootWrapper result = default(LootWrapper);
			result.InteractionLoot = i;
			return result;
		});
		list.AddRange(collection);
		return list;
	}
}
