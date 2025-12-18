using System.Collections.Generic;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeListNode : ActionNode
{
	private readonly GraphNodeListVariable m_Variable;

	private readonly GraphNodeListVariable m_NodeList;

	public SetGraphNodeListNode(GraphNodeListVariable variable, GraphNodeListVariable nodeList)
	{
		m_Variable = variable;
		m_NodeList = nodeList;
	}

	protected override void DoAction()
	{
		List<GraphNode> list = new List<GraphNode>();
		if (m_NodeList.Value != null)
		{
			list.AddRange(m_NodeList.Value);
		}
		m_Variable.Value = list;
	}
}
