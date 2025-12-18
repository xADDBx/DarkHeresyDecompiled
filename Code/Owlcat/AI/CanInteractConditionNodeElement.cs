using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Conditions/Can Interact Condition Leaf", "Can Interact Condition Leaf")]
[TypeId("85ba324b931b4071b71aba5b7ff85f3e")]
public class CanInteractConditionNodeElement : ConditionNodeElement<CanInteractConditionNode>
{
	public InteractableVariableReference Interactable;

	protected override CanInteractConditionNode CreateTypedNode(Blackboard blackboard)
	{
		InteractableVariable runtimeVariable = Interactable.GetRuntimeVariable(blackboard);
		return new CanInteractConditionNode(AbortType, runtimeVariable);
	}
}
