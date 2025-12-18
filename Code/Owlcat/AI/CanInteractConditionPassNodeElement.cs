using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Conditions/Can Interact Condition Pass Node", "Can Interact Condition Pass Node")]
[TypeId("0204469bb2d24f8ea2abf44d9a7f6cf3")]
public class CanInteractConditionPassNodeElement : ConditionPassNodeElement<CanInteractConditionPassNode>
{
	public InteractableVariableReference Interactable;

	protected override CanInteractConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		InteractableVariable runtimeVariable = Interactable.GetRuntimeVariable(blackboard);
		return new CanInteractConditionPassNode(AbortType, runtimeVariable);
	}
}
