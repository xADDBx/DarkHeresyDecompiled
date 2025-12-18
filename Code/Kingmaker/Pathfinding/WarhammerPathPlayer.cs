using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathPlayer : WarhammerPath<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>
{
	public static WarhammerPathPlayer Construct(AbstractUnitEntity unit, Vector3 start, float maxLength, [CanBeNull] GridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity, BlockMode blockMode, bool passThroughSmallUnits, WarhammerPathPlayerMetric initialLength, WarhammerPathPlayerMetricCostProvider traversalCostProvider, bool oneWayLinksAreForbidden, OnPathDelegate callback = null)
	{
		WarhammerPathPlayer warhammerPathPlayer = PathPool.GetPath<WarhammerPathPlayer>();
		warhammerPathPlayer.Setup(unit, start, maxLength, targetNode, targetEntity, blockMode, passThroughSmallUnits, initialLength, traversalCostProvider, oneWayLinksAreForbidden, callback);
		return warhammerPathPlayer;
	}

	protected override bool IsWithinRange(in WarhammerPathPlayerMetric node)
	{
		if (!(base.MaxLength < 0f))
		{
			return node.Length <= base.MaxLength;
		}
		return true;
	}

	protected override bool IsTargetNode(in WarhammerPathPlayerMetric distance, in GraphNode node)
	{
		if (node == base.TargetNode || (base.TargetEntity != null && WarhammerGeometryUtils.DistanceToInCells(node.Vector3Position(), base.Unit.SizeRect, base.TargetNode.Vector3Position(), base.Unit.SizeRect) < 2 && ((GridNodeBase)node).HasMeleeLos(base.TargetNode)))
		{
			return base.Unit.CanStandHere(node);
		}
		return false;
	}

	protected override GraphNode ClosestToTarget(GraphNode node1, GraphNode node2)
	{
		if (base.TargetNode == null)
		{
			return node2;
		}
		if (!((base.TargetNode.Vector3Position() - node1.Vector3Position()).sqrMagnitude < (base.TargetNode.Vector3Position() - node2.Vector3Position()).sqrMagnitude))
		{
			return node2;
		}
		return node1;
	}

	protected override WarhammerPathPlayerCell Convert(in WarhammerPathPlayerMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathPlayerCell(node.Vector3Position(), distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0), distance.IsOneWayPath);
	}

	protected override int Compare(in WarhammerPathPlayerMetric lengthA, in GraphNode nodeA, in WarhammerPathPlayerMetric lengthB, in GraphNode nodeB)
	{
		if (base.TargetNode != null)
		{
			return InformedSearchCompare(in lengthA, in nodeA, in lengthB, in nodeB);
		}
		return BreadthFirstSearchCompare(in lengthA, in nodeA, in lengthB, in nodeB);
	}

	private int BreadthFirstSearchCompare(in WarhammerPathPlayerMetric lengthA, in GraphNode nodeA, in WarhammerPathPlayerMetric lengthB, in GraphNode nodeB)
	{
		if (Math.Abs(lengthA.Length - lengthB.Length) >= 0.001f)
		{
			return Comparer<float>.Default.Compare(lengthA.Length, lengthB.Length);
		}
		if (lengthA.IsOneWayPath != lengthB.IsOneWayPath)
		{
			if (!lengthA.IsOneWayPath)
			{
				return -1;
			}
			return 1;
		}
		int num = Comparer<int>.Default.Compare(lengthA.PseudoCostOfFreeMovement, lengthB.PseudoCostOfFreeMovement);
		if (num != 0)
		{
			return num;
		}
		if (lengthA.DiagonalsCount == lengthB.DiagonalsCount && ConfigRoot.Instance.CombatRoot.LowerPriorityForLinkNodes)
		{
			return Comparer<int>.Default.Compare(lengthA.NavLinksCount, lengthB.NavLinksCount);
		}
		return Comparer<int>.Default.Compare(lengthB.DiagonalsCount, lengthA.DiagonalsCount);
	}

	private int InformedSearchCompare(in WarhammerPathPlayerMetric lengthA, in GraphNode nodeA, in WarhammerPathPlayerMetric lengthB, in GraphNode nodeB)
	{
		float num = CalculateTotalMetric(lengthA, nodeA);
		float num2 = CalculateTotalMetric(lengthB, nodeB);
		if (Math.Abs(num - num2) >= 0.001f)
		{
			return Comparer<float>.Default.Compare(num, num2);
		}
		if (lengthA.IsOneWayPath != lengthB.IsOneWayPath)
		{
			if (!lengthA.IsOneWayPath)
			{
				return -1;
			}
			return 1;
		}
		if (lengthA.PseudoCostOfFreeMovement != lengthB.PseudoCostOfFreeMovement)
		{
			return Comparer<int>.Default.Compare(lengthA.PseudoCostOfFreeMovement, lengthB.PseudoCostOfFreeMovement);
		}
		if (lengthA.DiagonalsCount == lengthB.DiagonalsCount && ConfigRoot.Instance.CombatRoot.LowerPriorityForLinkNodes)
		{
			return Comparer<int>.Default.Compare(lengthA.NavLinksCount, lengthB.NavLinksCount);
		}
		return Comparer<int>.Default.Compare(lengthB.DiagonalsCount, lengthA.DiagonalsCount);
	}

	private float CalculateTotalMetric(WarhammerPathPlayerMetric length, GraphNode node)
	{
		if (node is LinkNode ln)
		{
			return CalculateTotalMetricForLinkNode(length, ln);
		}
		return CalculateTotalMetricForGridNode(length, (GridNodeBase)node);
	}

	private float CalculateTotalMetricForLinkNode(WarhammerPathPlayerMetric length, LinkNode ln)
	{
		return EnumerateEndNodesForLinkNode(ln).Min((GridNodeBase gn) => CalculateTotalMetricForGridNode(length, gn));
	}

	private IEnumerable<GridNodeBase> EnumerateEndNodesForLinkNode(LinkNode ln)
	{
		GridNodeBase gridNodeBase = ((ln.linkConcrete.startLinkNode == ln) ? (ln.linkConcrete.startNodes[0] as GridNodeBase) : (ln.linkConcrete.endNodes[0] as GridNodeBase));
		GridNodeBase linkStartNode = ((ln.linkConcrete.startLinkNode != ln) ? (ln.linkConcrete.startNodes[0] as GridNodeBase) : (ln.linkConcrete.endNodes[0] as GridNodeBase));
		IEnumerable<GridNodeBase> enumerable = GraphHelper.TryGetAllSuitableNodesForUnitAfterLinkTraversal(gridNodeBase, linkStartNode, base.Unit);
		if (enumerable.Empty())
		{
			return new GridNodeBase[1] { gridNodeBase };
		}
		return enumerable;
	}

	private float CalculateTotalMetricForGridNode(WarhammerPathPlayerMetric length, GridNodeBase gridNode)
	{
		if (ConfigRoot.Instance.CombatRoot.UseManhattanHeuristic)
		{
			int num = Math.Abs(gridNode.XCoordinateInGrid - base.TargetNode.XCoordinateInGrid) + Math.Abs(gridNode.ZCoordinateInGrid - base.TargetNode.ZCoordinateInGrid);
			return length.Length + (float)num * base.Unit.Blueprint.WarhammerMovementApPerCell;
		}
		Vector2Int from = new Vector2Int(gridNode.XCoordinateInGrid, gridNode.ZCoordinateInGrid);
		Vector2Int to = new Vector2Int(base.TargetNode.XCoordinateInGrid, base.TargetNode.ZCoordinateInGrid);
		return length.Length + (float)WarhammerGeometryUtils.DistanceToInCells(from, default(IntRect), to, default(IntRect)) * base.Unit.Blueprint.WarhammerMovementApPerCell;
	}

	protected override void OnHeapExhausted()
	{
	}

	protected override void OnFoundEndNode(uint pathNode, uint hScore, uint gScore)
	{
	}
}
