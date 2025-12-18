using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathCharge : WarhammerPath<WarhammerPathChargeMetric, WarhammerPathChargeCell>
{
	public static WarhammerPathCharge Construct(AbstractUnitEntity unit, Vector3 start, [CanBeNull] GridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity, float maxLength, BlockMode blockMode, bool passThroughSmallUnits, WarhammerPathChargeMetric initialLength, ITraversalCostProvider<WarhammerPathChargeMetric> traversalCostProvider, OnPathDelegate callback = null)
	{
		WarhammerPathCharge warhammerPathCharge = PathPool.GetPath<WarhammerPathCharge>();
		warhammerPathCharge.Setup(unit, start, maxLength, targetNode, targetEntity, blockMode, passThroughSmallUnits, initialLength, traversalCostProvider, oneWayLinksAreForbidden: false, callback);
		return warhammerPathCharge;
	}

	protected override WarhammerPathChargeCell Convert(in WarhammerPathChargeMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathChargeCell(node.Vector3Position(), distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0));
	}

	protected override int Compare(in WarhammerPathChargeMetric lengthA, in GraphNode nodeA, in WarhammerPathChargeMetric lengthB, in GraphNode nodeB)
	{
		GridNodeBase gridNodeBase = (GridNodeBase)nodeA;
		GridNodeBase gridNodeBase2 = (GridNodeBase)nodeB;
		Vector2Int from = new Vector2Int(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid);
		Vector2Int from2 = new Vector2Int(gridNodeBase2.XCoordinateInGrid, gridNodeBase2.ZCoordinateInGrid);
		Vector2Int to = new Vector2Int(base.TargetNode.XCoordinateInGrid, base.TargetNode.ZCoordinateInGrid);
		float x = WarhammerGeometryUtils.DistanceToInCells(from, default(IntRect), to, default(IntRect));
		float y = WarhammerGeometryUtils.DistanceToInCells(from2, default(IntRect), to, default(IntRect));
		return Comparer<float>.Default.Compare(x, y);
	}

	protected override bool IsWithinRange(in WarhammerPathChargeMetric node)
	{
		if (node.Length < 1000f)
		{
			return node.Length <= base.MaxLength;
		}
		return false;
	}

	protected override bool IsTargetNode(in WarhammerPathChargeMetric distance, in GraphNode node)
	{
		if (node == base.TargetNode || (base.TargetEntity != null && WarhammerGeometryUtils.DistanceToInCells(node.Vector3Position(), base.Unit.SizeRect, base.TargetNode.Vector3Position(), default(IntRect)) < 2 && ((GridNodeBase)node).HasMeleeLos(base.TargetNode)))
		{
			return base.Unit.CanStandHere(node);
		}
		return false;
	}

	protected override GraphNode ClosestToTarget(GraphNode node1, GraphNode node2)
	{
		if (!((base.TargetNode.Vector3Position() - node1.Vector3Position()).sqrMagnitude < (base.TargetNode.Vector3Position() - node2.Vector3Position()).sqrMagnitude))
		{
			return node2;
		}
		return node1;
	}

	protected override void OnHeapExhausted()
	{
	}

	protected override void OnFoundEndNode(uint pathNode, uint hScore, uint gScore)
	{
	}
}
