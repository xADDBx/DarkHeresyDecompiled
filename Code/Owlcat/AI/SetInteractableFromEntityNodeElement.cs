using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Variables/Interactable/Set Interactable from Entity", "Set Interactable from Entity")]
[TypeId("a212082c3faa4bd4b9ce28bf6d6dd94a")]
public class SetInteractableFromEntityNodeElement : BehaviourTreeNodeElement<SetInteractableFromEntityNode>
{
	public InteractableVariableReference Variable;

	public EntityVariableReference Entity;

	protected override SetInteractableFromEntityNode CreateTypedNode(Blackboard blackboard)
	{
		InteractableVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Entity.GetRuntimeVariable(blackboard);
		return new SetInteractableFromEntityNode(runtimeVariable, runtimeVariable2);
	}
}
