using System.Collections.Generic;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeListWithSquareNode : BehaviourTreeNode
{
	private readonly GraphNodeListVariable m_Variable;

	private readonly GraphNodeVariable m_Center;

	private readonly int m_SquareSide;

	public SetGraphNodeListWithSquareNode(GraphNodeListVariable variable, GraphNodeVariable center, int squareSide)
	{
		m_Variable = variable;
		m_Center = center;
		m_SquareSide = squareSide;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetNodesInSquare((GridNodeBase)m_Center.Value, m_SquareSide);
		if (m_Variable.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<GraphNode> GetNodesInSquare(GridNodeBase center, int squareSide)
	{
		if (!(center?.Graph is GridGraph gridGraph))
		{
			return new List<GraphNode>();
		}
		int num = -squareSide;
		int xCoordinateInGrid = center.XCoordinateInGrid;
		int zCoordinateInGrid = center.ZCoordinateInGrid;
		List<GraphNode> list = new List<GraphNode>();
		for (int i = num; i <= squareSide; i++)
		{
			for (int j = num; j <= squareSide; j++)
			{
				GridNodeBase node = gridGraph.GetNode(xCoordinateInGrid + i, zCoordinateInGrid + j);
				if (node != null)
				{
					list.Add(node);
				}
			}
		}
		return list;
	}
}
