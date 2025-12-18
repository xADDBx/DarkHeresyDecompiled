using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Use Ability/Use Ability on Entity", "Use Ability on Entity")]
[TypeId("2ad0747d289a4b81b0e771e692c6fe94")]
public class UseAbilityOnEntityNodeElement : BehaviourTreeNodeElement<UseAbilityOnEntityNode>
{
	public AbilityVariableReference Ability;

	public EntityVariableReference Target;

	protected override UseAbilityOnEntityNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		EntityVariable runtimeVariable = Target.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable2 = Ability.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new UseAbilityOnEntityNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeInternalDataVariable);
	}
}
