using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathAi : WarhammerPath<WarhammerPathAiMetric, WarhammerPathAiCell>
{
	public static WarhammerPathAi Construct(AbstractUnitEntity unit, Vector3 start, float maxLength, [CanBeNull] GridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity, BlockMode blockMode, bool passThroughSmallUnits, WarhammerPathAiMetric initialLength, ITraversalCostProvider<WarhammerPathAiMetric> traversalCostProvider, OnPathDelegate callback = null)
	{
		WarhammerPathAi warhammerPathAi = PathPool.GetPath<WarhammerPathAi>();
		warhammerPathAi.Setup(unit, start, maxLength, targetNode, targetEntity, blockMode, passThroughSmallUnits, initialLength, traversalCostProvider, oneWayLinksAreForbidden: false, callback);
		return warhammerPathAi;
	}

	protected override WarhammerPathAiCell Convert(in WarhammerPathAiMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathAiCell(node.Vector3Position(), distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0), distance.EnteredAoE, distance.LeavedAoE, distance.StepsInsideDamagingAoE, distance.ProvokedAttacks);
	}

	protected override int Compare(in WarhammerPathAiMetric lengthA, in GraphNode nodeA, in WarhammerPathAiMetric lengthB, in GraphNode nodeB)
	{
		return Comparer<float>.Default.Compare(lengthA.Delay, lengthB.Delay);
	}

	protected override bool IsWithinRange(in WarhammerPathAiMetric node)
	{
		return node.Length <= base.MaxLength;
	}

	protected override bool IsTargetNode(in WarhammerPathAiMetric distance, in GraphNode node)
	{
		if (base.TargetNode != null)
		{
			return base.TargetNode == node;
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

	protected override void OnHeapExhausted()
	{
	}

	protected override void OnFoundEndNode(uint pathNode, uint hScore, uint gScore)
	{
	}
}
