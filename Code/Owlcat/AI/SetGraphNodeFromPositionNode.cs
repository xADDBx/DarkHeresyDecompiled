using Kingmaker.Pathfinding;
using Owlcat.BehaviourTrees;
using UnityEngine;

namespace Owlcat.AI;

public class SetGraphNodeFromPositionNode : BehaviourTreeNode
{
	private readonly GraphNodeVariable m_Variable;

	private readonly PositionVariable m_Position;

	public SetGraphNodeFromPositionNode(GraphNodeVariable variable, PositionVariable position)
	{
		m_Variable = variable;
		m_Position = position;
	}

	public override NodeVisitResult ForwardVisit()
	{
		Vector3 value = m_Position.Value;
		m_Variable.Value = value.GetNearestNodeXZUnwalkable();
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}
}
