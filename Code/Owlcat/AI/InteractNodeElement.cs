using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Interact", "Interact")]
[TypeId("eb10fb27e5ce4f9985de9de81a460bdb")]
public class InteractNodeElement : BehaviourTreeNodeElement<InteractNode>
{
	public InteractableVariableReference Interactable;

	protected override InteractNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		InteractableVariable runtimeVariable = Interactable.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new InteractNode(agentVariable, runtimeVariable, runtimeInternalDataVariable);
	}
}
