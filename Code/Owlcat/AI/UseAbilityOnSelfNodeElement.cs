using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Use Ability/Use Ability on Self", "Use Ability on Self")]
[TypeId("556d2999310441c480a0e49ce4345ebe")]
public class UseAbilityOnSelfNodeElement : BehaviourTreeNodeElement<UseAbilityOnSelfNode>
{
	public AbilityVariableReference Ability;

	protected override UseAbilityOnSelfNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		AbilityVariable runtimeVariable = Ability.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new UseAbilityOnSelfNode(agentVariable, runtimeVariable, runtimeInternalDataVariable);
	}
}
