using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Variables/Interactable/Set Interactable", "Set Interactable")]
[TypeId("1426698d6b62451989e345af67185b09")]
public class SetInteractableNodeElement : BehaviourTreeNodeElement<SetInteractableNode>
{
	public InteractableVariableReference Variable;

	public InteractableVariableReference Value;

	protected override SetInteractableNode CreateTypedNode(Blackboard blackboard)
	{
		InteractableVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		InteractableVariable runtimeVariable2 = Value.GetRuntimeVariable(blackboard);
		return new SetInteractableNode(runtimeVariable, runtimeVariable2);
	}
}
