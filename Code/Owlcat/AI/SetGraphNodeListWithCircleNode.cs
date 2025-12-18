using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeListWithCircleNode : BehaviourTreeNode
{
	private readonly GraphNodeListVariable m_Variable;

	private readonly GraphNodeVariable m_Center;

	private readonly int m_Radius;

	public SetGraphNodeListWithCircleNode(GraphNodeListVariable variable, GraphNodeVariable center, int radius)
	{
		m_Variable = variable;
		m_Center = center;
		m_Radius = radius;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetNodesInCircle((GridNodeBase)m_Center.Value, m_Radius);
		if (m_Variable.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<GraphNode> GetNodesInCircle(GridNodeBase center, int radius)
	{
		if (center == null)
		{
			return new List<GraphNode>();
		}
		Queue<GridNodeBase> queue = new Queue<GridNodeBase>();
		queue.Enqueue(center);
		IntRect intRect = default(IntRect);
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		GridNodeBase result;
		while (queue.TryDequeue(out result))
		{
			if (!hashSet.Contains(result) && result != null && WarhammerGeometryUtils.DistanceToInCells(center.Vector3Position(), intRect, result.Vector3Position(), intRect) <= radius)
			{
				hashSet.Add(result);
				for (int i = 0; i < 8; i++)
				{
					queue.Enqueue(result.GetNeighbourAlongDirection(i, checkConnectivity: false));
				}
			}
		}
		return hashSet.ToList();
	}
}
