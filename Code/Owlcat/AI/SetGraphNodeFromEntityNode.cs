using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SetGraphNodeFromEntityNode : BehaviourTreeNode
{
	private readonly GraphNodeVariable m_Variable;

	private readonly EntityVariable m_Entity;

	public SetGraphNodeFromEntityNode(GraphNodeVariable variable, EntityVariable entity)
	{
		m_Variable = variable;
		m_Entity = entity;
	}

	public override NodeVisitResult ForwardVisit()
	{
		MechanicEntity value = m_Entity.Value;
		m_Variable.Value = value?.GetNearestNodeXZ();
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}
}
