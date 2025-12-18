using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Move/Move to Interactable", "Move to Interactable")]
[TypeId("6763f6d3d31b420c8347e57aba7dc6f8")]
public class MoveToInteractableNodeElement : BehaviourTreeNodeElement<MoveToInteractableNode>
{
	public InteractableVariableReference Interactable;

	public AiThreatsHandlingStrategy ThreatsHandlingStrategy;

	protected override MoveToInteractableNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		InteractableVariable runtimeVariable = Interactable.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new MoveToInteractableNode(agentVariable, runtimeVariable, runtimeInternalDataVariable, ThreatsHandlingStrategy);
	}
}
