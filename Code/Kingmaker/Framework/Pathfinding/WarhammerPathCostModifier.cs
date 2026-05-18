using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine.Pool;

namespace Kingmaker.Framework.Pathfinding;

public sealed class WarhammerPathCostModifier
{
	public sealed class InvalidateCacheController : IControllerTick, IController, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, IEntityLostFactHandler, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>
	{
		TickType IControllerTick.GetTickType()
		{
			return TickType.Simulation;
		}

		void IControllerTick.Tick()
		{
			if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				InvalidateCache();
			}
		}

		void IEntityPositionChangedHandler.HandleEntityPositionChanged()
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity != null && mechanicEntity.IsInCombat)
			{
				InvalidateCache();
			}
		}

		void IEntityGainFactHandler.HandleEntityGainFact(EntityFact fact)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity != null && mechanicEntity.IsInCombat)
			{
				InvalidateCache();
			}
		}

		void IEntityLostFactHandler.HandleEntityLostFact(EntityFact fact)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity != null && mechanicEntity.IsInCombat)
			{
				InvalidateCache();
			}
		}

		void IUnitCombatHandler.HandleUnitJoinCombat()
		{
			InvalidateCache();
		}

		void IUnitCombatHandler.HandleUnitLeaveCombat()
		{
			InvalidateCache();
		}
	}

	private static readonly Dictionary<AbstractUnitEntity, WarhammerPathCostModifier> _cache = new Dictionary<AbstractUnitEntity, WarhammerPathCostModifier>();

	private readonly HashSet<GraphNode> _threateningArea = new HashSet<GraphNode>();

	private readonly HashSet<GraphNode> _forbiddenArea = new HashSet<GraphNode>();

	private readonly HashSet<GraphNode> _freeArea = new HashSet<GraphNode>();

	private readonly Dictionary<GraphNode, float> _overrideCosts = new Dictionary<GraphNode, float>();

	public static WarhammerPathCostModifier Get(AbstractUnitEntity unit)
	{
		if (!_cache.TryGetValue(unit, out var value))
		{
			value = (_cache[unit] = new WarhammerPathCostModifier(unit));
		}
		return value;
	}

	public static void InvalidateCache()
	{
		_cache.Clear();
	}

	private WarhammerPathCostModifier(AbstractUnitEntity unit)
	{
		if (!unit.Features.IgnoreThreateningAreaForMovementCostCalculation)
		{
			CollectThreateningAreaNodes(unit, _threateningArea);
		}
		if ((bool)unit.Features.CanNotMoveCloserToEnemies)
		{
			CollectAllNodesCloserToEnemyThanCurrentNode(unit, _forbiddenArea);
		}
		if ((bool)unit.Features.FreeMovementNearParty)
		{
			CollectNodesAroundAllies(unit, _freeArea);
		}
		NodeTraverseCostHelper.CollectOverrideCosts(unit, _overrideCosts);
	}

	public bool IsFree(GraphNode node)
	{
		return _freeArea.Contains(node);
	}

	public float? GetOverrideCost(GraphNode node)
	{
		if (!_overrideCosts.TryGetValue(node, out var value))
		{
			return null;
		}
		return value;
	}

	public bool IsForbidden(GraphNode node)
	{
		return _forbiddenArea.Contains(node);
	}

	public bool IsThreatening(GraphNode node)
	{
		return _threateningArea.Contains(node);
	}

	private static void CollectThreateningAreaNodes(AbstractUnitEntity entity, HashSet<GraphNode> threateningArea)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		foreach (UnitGroupMemory.UnitInfo enemy in baseUnitEntity.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit = enemy.Unit;
			if ((!Game.IsTestRun || unit != null) && unit.CanMakeAttackOfOpportunity(baseUnitEntity))
			{
				unit.CollectThreateningAreaNodes(threateningArea);
			}
		}
	}

	private static void CollectAllNodesCloserToEnemyThanCurrentNode(AbstractUnitEntity unit, HashSet<GraphNode> result)
	{
		List<GraphNode> value;
		using (CollectionPool<List<GraphNode>, GraphNode>.Get(out value))
		{
			foreach (BaseUnitEntity item in Game.Instance.EntityPools.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy))
			{
				int num = WarhammerGeometryUtils.DistanceToInCells(unit.Position, unit.SizeRect, item.Position, item.SizeRect) - 1;
				foreach (GridNodeBase item2 in GridAreaHelper.GetNodesSpiralAround(item.Position.GetNearestNodeXZUnwalkable(), item.SizeRect, num))
				{
					if (item.DistanceToInCells(item2.Vector3Position()) <= num)
					{
						value.Add(item2);
					}
				}
			}
			CombatHUDRenderer.ExtendMovementAreaByUnitSize(value, unit.SizeRect, extendToNorthEast: false);
			result.AddRange(value);
		}
	}

	private static void CollectNodesAroundAllies(AbstractUnitEntity unit, HashSet<GraphNode> result)
	{
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
		{
			if (!allBaseAwakeUnit.IsInCombat || allBaseAwakeUnit == unit || !allBaseAwakeUnit.IsInPlayerParty)
			{
				continue;
			}
			foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(allBaseAwakeUnit.CurrentUnwalkableNode, allBaseAwakeUnit.SizeRect, 1))
			{
				result.Add(item);
			}
			foreach (GridNodeBase occupiedNode in allBaseAwakeUnit.GetOccupiedNodes())
			{
				result.Add(occupiedNode);
			}
		}
	}
}
