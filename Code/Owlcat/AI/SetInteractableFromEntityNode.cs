using Kingmaker;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SetInteractableFromEntityNode : BehaviourTreeNode
{
	private readonly InteractableVariable m_Variable;

	private readonly EntityVariable m_Entity;

	public SetInteractableFromEntityNode(InteractableVariable variable, EntityVariable entity)
	{
		m_Variable = variable;
		m_Entity = entity;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetInteractionActionFromEntity(m_Entity.Value);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static InteractionAction GetInteractionActionFromEntity(Entity entity)
	{
		InteractionActionPart interactionActionPart = entity?.Parts.GetOptional<InteractionActionPart>();
		if (interactionActionPart == null)
		{
			PFLog.AI.Warning($"Entity {entity} doesn't have InteractionActionPart");
			return null;
		}
		return (InteractionAction)interactionActionPart.Source;
	}
}
