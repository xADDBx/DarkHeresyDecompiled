using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class CombatEngagementHelper
{
	private struct EntityPathKey
	{
		public EntityRef<BaseUnitEntity> EntityRef;

		public GraphNode StartNode;

		public GraphNode EndNode;

		public override bool Equals(object obj)
		{
			if (obj is EntityPathKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EntityRef, StartNode, EndNode);
		}

		private bool Equals(EntityPathKey other)
		{
			if (EqualityComparer<EntityRef<BaseUnitEntity>>.Default.Equals(EntityRef, other.EntityRef) && EqualityComparer<GraphNode>.Default.Equals(StartNode, other.StartNode))
			{
				return EqualityComparer<GraphNode>.Default.Equals(EndNode, other.EndNode);
			}
			return false;
		}
	}

	private static readonly Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> Engage = new Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>>();

	private static readonly Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> EngagedBy = new Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>>();

	private static readonly Dictionary<EntityPathKey, List<EntityRef<BaseUnitEntity>>> EngagedByAlongPath = new Dictionary<EntityPathKey, List<EntityRef<BaseUnitEntity>>>();

	private static int s_SystemStepIndex;

	public static void DropCacheIfNecessary()
	{
		if (s_SystemStepIndex == Game.Instance.RealTimeController.CurrentSystemStepIndex)
		{
			return;
		}
		foreach (KeyValuePair<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> item in Engage)
		{
			ReleaseList(item.Value);
		}
		Engage.Clear();
		foreach (KeyValuePair<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> item2 in EngagedBy)
		{
			ReleaseList(item2.Value);
		}
		EngagedBy.Clear();
		foreach (KeyValuePair<EntityPathKey, List<EntityRef<BaseUnitEntity>>> item3 in EngagedByAlongPath)
		{
			ReleaseList(item3.Value);
		}
		EngagedByAlongPath.Clear();
		s_SystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
	}

	public static IEnumerable<BaseUnitEntity> GetEngagedUnits(this BaseUnitEntity unit)
	{
		return Dereference(GetEngageListInternal(unit));
	}

	public static IEnumerable<BaseUnitEntity> GetEngagedByUnits(this BaseUnitEntity unit)
	{
		return Dereference(GetEngagedByListInternal(unit));
	}

	public static IEnumerable<BaseUnitEntity> GetEngagedByUnitsAlongPath(this BaseUnitEntity unit, IReadOnlyList<GraphNode> path)
	{
		return Dereference(GetEngagedByAlongPathListInternal(unit, path));
	}

	public static bool IsEngage(this BaseUnitEntity attacker, BaseUnitEntity target)
	{
		return GetEngageListInternal(attacker).Contains(target);
	}

	public static bool IsEngagedBy(this BaseUnitEntity target, BaseUnitEntity attacker)
	{
		return GetEngagedByListInternal(target).Contains(attacker);
	}

	private static List<EntityRef<BaseUnitEntity>> ClaimList()
	{
		return ListPool<EntityRef<BaseUnitEntity>>.Claim();
	}

	private static void ReleaseList(List<EntityRef<BaseUnitEntity>> list)
	{
		ListPool<EntityRef<BaseUnitEntity>>.Release(list);
	}

	private static List<EntityRef<BaseUnitEntity>> GetEngageListInternal(BaseUnitEntity unit)
	{
		DropCacheIfNecessary();
		return Engage.Get(unit) ?? CollectEngage(unit);
	}

	private static List<EntityRef<BaseUnitEntity>> GetEngagedByListInternal(BaseUnitEntity unit)
	{
		DropCacheIfNecessary();
		return EngagedBy.Get(unit) ?? CollectEngagedBy(unit);
	}

	private static IReadOnlyList<EntityRef<BaseUnitEntity>> GetEngagedByAlongPathListInternal(BaseUnitEntity unit, IReadOnlyList<GraphNode> path)
	{
		DropCacheIfNecessary();
		EntityPathKey key = ConstructPathKey(unit, path);
		IReadOnlyList<EntityRef<BaseUnitEntity>> readOnlyList = EngagedByAlongPath.Get(key);
		return readOnlyList ?? CollectEngagedByAlongPath(unit, path, key);
	}

	private static EntityPathKey ConstructPathKey(BaseUnitEntity unit, IReadOnlyList<GraphNode> path)
	{
		EntityPathKey result = default(EntityPathKey);
		result.EntityRef = unit;
		result.StartNode = ((path.Count > 0) ? path[0] : null);
		object endNode;
		if (path.Count <= 0)
		{
			endNode = null;
		}
		else
		{
			endNode = path[path.Count - 1];
		}
		result.EndNode = (GraphNode)endNode;
		return result;
	}

	private static IEnumerable<BaseUnitEntity> Dereference(IEnumerable<EntityRef<BaseUnitEntity>> list)
	{
		foreach (EntityRef<BaseUnitEntity> item in list)
		{
			if (item.Entity != null)
			{
				yield return item.Entity;
			}
		}
	}

	public static List<EntityRef<BaseUnitEntity>> CollectUnitsAround(BaseUnitEntity unit, [CanBeNull] Func<BaseUnitEntity, bool> predicate = null)
	{
		List<EntityRef<BaseUnitEntity>> list = ClaimList();
		GridNode currentUnwalkableNode = unit.CurrentUnwalkableNode;
		if (currentUnwalkableNode == null)
		{
			PFLog.Default.Warning($"Cannot collect engage list! Null origin node for unit {unit}");
			return list;
		}
		FillUnitsAround(in list, currentUnwalkableNode, unit.SizeRect, predicate);
		return list;
	}

	private static List<EntityRef<BaseUnitEntity>> CollectUnitsAlongPath(BaseUnitEntity unit, IReadOnlyList<GraphNode> path, Func<BaseUnitEntity, GridNodeBase, bool> predicate)
	{
		List<EntityRef<BaseUnitEntity>> list = ClaimList();
		foreach (GraphNode item in path)
		{
			GridNodeBase gridNode = item as GridNodeBase;
			if (gridNode != null)
			{
				FillUnitsAround(in list, gridNode, unit.SizeRect, (BaseUnitEntity e) => predicate(e, gridNode));
			}
		}
		return list;
	}

	private static void FillUnitsAround(in List<EntityRef<BaseUnitEntity>> list, GridNodeBase node, IntRect unitSize, [CanBeNull] Func<BaseUnitEntity, bool> predicate = null)
	{
		foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(node, unitSize, 1))
		{
			BaseUnitEntity firstUnit = item.GetFirstUnit();
			if (firstUnit != null && (predicate == null || predicate(firstUnit)))
			{
				list.Add(firstUnit);
			}
		}
	}

	private static List<EntityRef<BaseUnitEntity>> CollectEngage(BaseUnitEntity unit)
	{
		return Engage[unit] = CollectUnitsAround(unit, (BaseUnitEntity entity) => unit.IsThreat(entity));
	}

	private static List<EntityRef<BaseUnitEntity>> CollectEngagedBy(BaseUnitEntity unit)
	{
		return EngagedBy[unit] = CollectUnitsAround(unit, (BaseUnitEntity entity) => entity.IsThreat(unit));
	}

	private static IReadOnlyList<EntityRef<BaseUnitEntity>> CollectEngagedByAlongPath(BaseUnitEntity unit, IReadOnlyList<GraphNode> path, EntityPathKey key)
	{
		return EngagedByAlongPath[key] = CollectUnitsAlongPath(unit, path, (BaseUnitEntity entity, GridNodeBase node) => entity.IsThreat(unit, node));
	}

	public static bool IsEngagedInPosition(this BaseUnitEntity unit, Vector3 desiredPosition)
	{
		GridNodeBase nearestNodeXZUnwalkable = desiredPosition.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null)
		{
			PFLog.Default.Warning($"Null origin node for unit {unit}");
			return false;
		}
		foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable, unit.SizeRect, 1))
		{
			BaseUnitEntity firstUnit = item.GetFirstUnit();
			if (firstUnit != null && firstUnit.IsThreat(unit))
			{
				return true;
			}
		}
		return false;
	}
}
