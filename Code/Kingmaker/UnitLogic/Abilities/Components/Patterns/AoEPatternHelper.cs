using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public static class AoEPatternHelper
{
	private readonly struct NodeCollector : Linecast.ICanTransitionBetweenCells
	{
		private readonly List<GridNodeBase> m_NodesOnLine;

		public NodeCollector(List<GridNodeBase> nodesOnLine)
		{
			m_NodesOnLine = nodesOnLine;
		}

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			m_NodesOnLine.Add(nodeTo);
			return true;
		}
	}

	public static GridNodeBase GetActualCastNode([NotNull] MechanicEntity caster, Vector3 castPosition, Vector3 target, int minRange = 0, int maxRange = int.MaxValue)
	{
		return GetActualCastNode(caster, castPosition.GetNearestNodeXZUnwalkable(), target, minRange, maxRange);
	}

	public static GridNodeBase GetActualCastNode([NotNull] MechanicEntity caster, GridNodeBase castNode, GridNodeBase targetNode, int minRange = 0, int maxRange = int.MaxValue)
	{
		return GetActualCastNode(caster, castNode, targetNode.Vector3Position(), minRange, maxRange);
	}

	public static Vector3 GetActualCastPosition([NotNull] MechanicEntity caster, Vector3 castPosition, Vector3 target, int minRange, int maxRange)
	{
		return GetActualCastNode(caster, castPosition, target, minRange, maxRange).Vector3Position();
	}

	public static Vector3 GetActualCastPosition(MechanicEntity caster, GridNodeBase casterPosition, Vector3 target, int minRange, int maxRange)
	{
		return GetActualCastNode(caster, casterPosition, target, minRange, maxRange).Vector3Position();
	}

	public static GridNodeBase GetActualCastNode([NotNull] MechanicEntity casterEntity, GridNodeBase castNode, Vector3 target, int minRange, int maxRange)
	{
		GridNodeBase innerNodeNearestToTarget = casterEntity.GetInnerNodeNearestToTarget(castNode, target);
		NavGraph graph = castNode.Graph;
		List<GridNodeBase> list = TempList.Get<GridNodeBase>();
		list.Add(innerNodeNearestToTarget);
		NodeCollector condition = new NodeCollector(list);
		Linecast.LinecastGrid(graph, innerNodeNearestToTarget.Vector3Position(), target, innerNodeNearestToTarget, out var hit, ref condition);
		int i = list.Count - 1;
		while (i >= 0 && innerNodeNearestToTarget.CellDistanceTo(list[i]) > maxRange)
		{
			i--;
		}
		int num = innerNodeNearestToTarget.CellDistanceTo(list[i]);
		if (num < minRange)
		{
			list.Clear();
			Vector3 vector = innerNodeNearestToTarget.Vector3Position();
			if ((double)(target - vector).sqrMagnitude < 0.0001)
			{
				target += Vector3.back;
			}
			target = vector + (target - vector).normalized * (minRange * 2);
			Linecast.LinecastGrid(graph, vector, target, castNode, out hit, ref condition);
			for (i = 0; i < list.Count - 1 && innerNodeNearestToTarget.CellDistanceTo(list[i]) < minRange; i++)
			{
			}
		}
		bool num2 = list.Empty();
		num = ((!num2) ? innerNodeNearestToTarget.CellDistanceTo(list[i]) : 0);
		if (num2 || num < minRange || num > maxRange)
		{
			return castNode;
		}
		return list[i];
	}

	public static bool WouldTargetEntityPattern(MechanicEntity entity, OrientedPatternData pattern, Vector3 castPosition, float distance, out LosDescription los)
	{
		if ((entity.Position - castPosition).sqrMagnitude > distance * distance)
		{
			los = new LosDescription(LosCalculations.CoverType.Obstacle);
			return false;
		}
		los = LosCalculations.GetWarhammerLos(castPosition, default(IntRect), entity);
		return WouldTargetEntity(pattern, entity);
	}

	public static bool WouldTargetEntity(OrientedPatternData coveredNodes, MechanicEntity entity)
	{
		if (entity is DestructibleEntity { CurrentStageGridObstacles: { } currentStageGridObstacles } destructibleEntity && currentStageGridObstacles.Length > 0)
		{
			return WouldTargetDestructibleEntity(coveredNodes, destructibleEntity);
		}
		foreach (GridNodeBase occupiedNode in entity.GetOccupiedNodes())
		{
			if (coveredNodes.Contains(occupiedNode))
			{
				return true;
			}
		}
		return false;
	}

	private static bool WouldTargetDestructibleEntity(OrientedPatternData coveredNodes, DestructibleEntity destructibleEntity)
	{
		GridGraph gridGraph = AstarPath.active.data.gridGraph;
		GridObstacle[] currentStageGridObstacles = destructibleEntity.CurrentStageGridObstacles;
		for (int i = 0; i < currentStageGridObstacles.Length; i++)
		{
			(GridNodeIndex forwardNode, GridNodeIndex backwardNode) affectedNodes = currentStageGridObstacles[i].GetAffectedNodes(gridGraph.transform);
			GridNodeIndex item = affectedNodes.forwardNode;
			GridNodeIndex item2 = affectedNodes.backwardNode;
			GridNodeBase node = gridGraph.GetNode(item.x, item.z);
			GridNodeBase node2 = gridGraph.GetNode(item2.x, item2.z);
			GridNodeIndex gridNodeIndex = (((item - item2).x > 0) ? new GridNodeIndex(0, 1) : new GridNodeIndex(1, 0));
			if (coveredNodes.ContainsAny(node))
			{
				if (coveredNodes.Contains(node2))
				{
					return true;
				}
				GridNodeBase node3 = gridGraph.GetNode(item2.x - gridNodeIndex.x, item2.z - gridNodeIndex.z);
				if (coveredNodes.Contains(node3))
				{
					return true;
				}
				GridNodeBase node4 = gridGraph.GetNode(item2.x + gridNodeIndex.x, item2.z + gridNodeIndex.z);
				if (coveredNodes.Contains(node4))
				{
					return true;
				}
			}
			if (coveredNodes.ContainsAny(node2))
			{
				if (coveredNodes.Contains(node))
				{
					return true;
				}
				GridNodeBase node5 = gridGraph.GetNode(item.x - gridNodeIndex.x, item.z - gridNodeIndex.z);
				if (coveredNodes.Contains(node5))
				{
					return true;
				}
				GridNodeBase node6 = gridGraph.GetNode(item.x + gridNodeIndex.x, item.z + gridNodeIndex.z);
				if (coveredNodes.Contains(node6))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static GridNodeBase GetGridNode(Vector3 pos)
	{
		return (GridNodeBase)ObstacleAnalyzer.GetNearestNode(pos).node;
	}

	public static Vector3 GetGridAdjustedPosition(Vector3 pos)
	{
		GraphNode node = ObstacleAnalyzer.GetNearestNode(pos).node;
		if (node != null)
		{
			return (Vector3)node.position;
		}
		return pos;
	}

	public static OrientedPatternData GetOrientedPattern([CanBeNull] IAbilityDataProviderForPattern ability, [NotNull] MechanicEntity caster, [NotNull] AoEPattern pattern, [NotNull] IAbilityAoEPatternProvider provider, [NotNull] GridNodeBase casterNode, [NotNull] GridNodeBase targetNode, bool castOnSameLevel, bool directional, bool coveredTargetsOnly, Size targetSize, out GridNodeBase actualCastNode, int? builtInHaloSize = null, int? haloSize = null)
	{
		actualCastNode = GetActualCastNode(caster, casterNode, targetNode);
		GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(casterNode, targetNode.Vector3Position());
		GridNodeBase outerNodeNearestToTarget = caster.GetOuterNodeNearestToTarget(casterNode, targetNode.Vector3Position());
		Vector3 vector = actualCastNode.Vector3Position();
		if (castOnSameLevel && Mathf.Abs(innerNodeNearestToTarget.Vector3Position().y - vector.y) > AoEPattern.SameLevelDiff)
		{
			return OrientedPatternData.Empty;
		}
		using (ProfileScope.New("GetOriented"))
		{
			GridNodeBase checkLosFromNode = (provider.CalculateAttackFromPatternCentre ? actualCastNode : innerNodeNearestToTarget);
			GridNodeBase gridNodeBase = ((directional || (ability != null && ability.IsBurst)) ? outerNodeNearestToTarget : actualCastNode);
			Vector3 castDirection = AoEPattern.GetCastDirection(pattern.Type, innerNodeNearestToTarget, gridNodeBase, targetNode);
			AoEPatternCalculation @params = new AoEPatternCalculation(checkLosFromNode, gridNodeBase, castDirection).SetIgnoreLos(provider.IsIgnoreLos).SetRespectCovers(provider.RespectCovers, provider.RespectCoversEvenInCloseRange).SetIgnoreLevelDifference(provider.IsIgnoreLevelDifference)
				.SetDirectional(directional)
				.SetCoveredTargetsOnly(coveredTargetsOnly)
				.SetUseMeleeLos(provider.UseMeleeLos)
				.SetEntitySizeRect(targetSize)
				.SetBuiltInHaloSize(builtInHaloSize)
				.SetHaloSize(haloSize);
			return pattern.GetOriented(@params);
		}
	}
}
