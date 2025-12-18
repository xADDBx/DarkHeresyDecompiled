using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Use Ability/Use Ability on GraphNode", "Use Ability on GraphNode")]
[TypeId("ede701629a364769afb616d1c5fa88b9")]
public class UseAbilityOnGraphNodeNodeElement : BehaviourTreeNodeElement<UseAbilityOnGraphNodeNode>
{
	public AbilityVariableReference Ability;

	public GraphNodeVariableReference TargetNode;

	protected override UseAbilityOnGraphNodeNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeVariable runtimeVariable = TargetNode.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable2 = Ability.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new UseAbilityOnGraphNodeNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeInternalDataVariable);
	}
}
