using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathPlayerMetricCostProvider : ITraversalCostProvider<WarhammerPathPlayerMetric>
{
	private readonly AbstractUnitEntity m_Unit;

	private readonly bool m_IgnoreThreateningAreaCost;

	private readonly WarhammerPathCostModifier m_CostModifier;

	public WarhammerPathPlayerMetricCostProvider(AbstractUnitEntity unit, bool ignoreThreateningAreaCost)
	{
		m_Unit = unit;
		m_IgnoreThreateningAreaCost = ignoreThreateningAreaCost;
		m_CostModifier = WarhammerPathCostModifier.Get(unit);
	}

	public WarhammerPathPlayerMetric Calc(in WarhammerPathPlayerMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		if (from is LinkNode linkNode && to is LinkNode linkNode2)
		{
			float num = 1f;
			if (ConfigRoot.Instance.CombatRoot.UseIncreasedCostForLeaps)
			{
				GridNodeBase gridNodeBase = linkNode2.linkConcrete.startNodes[0] as GridNodeBase;
				GridNodeBase gridNodeBase2 = linkNode2.linkConcrete.endNodes[0] as GridNodeBase;
				Vector2Int from2 = new Vector2Int(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid);
				Vector2Int to2 = new Vector2Int(gridNodeBase2.XCoordinateInGrid, gridNodeBase2.ZCoordinateInGrid);
				num = WarhammerGeometryUtils.DistanceToInCells(from2, default(IntRect), to2, default(IntRect));
			}
			if (ConfigRoot.Instance.CombatRoot.UseIncreasedCostForLargeCreatures && m_Unit.Size == Size.Large)
			{
				num += 1f;
			}
			num = Math.Max(linkNode2.linkSource.costFactor, num);
			bool isOneWayPath = distanceFrom.IsOneWayPath || linkNode.linkSource.component is WarhammerOneWayNodeLink;
			return new WarhammerPathPlayerMetric(distanceFrom.DiagonalsCount, distanceFrom.DiagonalsCount, distanceFrom.NavLinksCount + 1, distanceFrom.Length + num, distanceFrom.PseudoCostOfFreeMovement + 1, isOneWayPath);
		}
		if (from is LinkNode && to is GridNodeBase)
		{
			return distanceFrom;
		}
		if (from is GridNodeBase && to is LinkNode)
		{
			return distanceFrom;
		}
		bool flag = PathExtras.IsDiagonal((GridNodeBase)to, (GridNodeBase)from);
		float num2 = ((!(distanceFrom.DiagonalsCount % 2 == 1 && flag)) ? 1 : 2);
		float num3 = Calc(from, to);
		bool flag2 = num3 == 0f;
		num2 *= num3;
		float length = distanceFrom.Length + num2;
		return new WarhammerPathPlayerMetric(distanceFrom.DiagonalsCount + ((flag && num2 > float.Epsilon) ? 1 : 0), distanceFrom.DiagonalsCountTotal + (flag ? 1 : 0), distanceFrom.NavLinksCount, length, distanceFrom.PseudoCostOfFreeMovement + (flag2 ? ((!(distanceFrom.DiagonalsCountTotal % 2 == 1 && flag)) ? 1 : 2) : 0), distanceFrom.IsOneWayPath);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		if (m_CostModifier.IsFree(to))
		{
			return 0f;
		}
		if (m_CostModifier.IsForbidden(to))
		{
			return ConfigRoot.Instance.CombatRoot.ForbiddenNodesTraverseCost;
		}
		float num = m_CostModifier.GetOverrideCost(to) ?? ((!m_IgnoreThreateningAreaCost && m_CostModifier.IsThreatening(from)) ? m_Unit.GetWarhammerMovementApPerCellThreateningArea() : m_Unit.Blueprint.WarhammerMovementApPerCell);
		if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
		{
			num *= currentLink.CostFactor;
		}
		return num;
	}

	WarhammerPathPlayerMetric ITraversalCostProvider<WarhammerPathPlayerMetric>.Calc(in WarhammerPathPlayerMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}
}
