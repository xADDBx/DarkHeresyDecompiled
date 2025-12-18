using System.Collections.Generic;
using System.Linq;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeListWithIntersectionNode : BehaviourTreeNode
{
	private readonly GraphNodeListVariable m_Variable;

	private readonly GraphNodeListVariable m_NodeList1;

	private readonly GraphNodeListVariable m_NodeList2;

	public SetGraphNodeListWithIntersectionNode(GraphNodeListVariable variable, GraphNodeListVariable nodeList1, GraphNodeListVariable nodeList2)
	{
		m_Variable = variable;
		m_NodeList1 = nodeList1;
		m_NodeList2 = nodeList2;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = Intersect(m_NodeList1.Value, m_NodeList2.Value);
		if (m_Variable.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<GraphNode> Intersect(List<GraphNode> list1, List<GraphNode> list2)
	{
		List<GraphNode> list3 = new List<GraphNode>();
		if (list1 == null || list2 == null || list1.Count == 0 || list2.Count == 0)
		{
			return list3;
		}
		list3.AddRange(list1.Where(list2.Contains));
		return list3;
	}
}
