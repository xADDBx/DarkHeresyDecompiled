using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Use Ability/Use Ability on Body Part", "Use Ability on Body Part")]
[TypeId("832a3650d672493d815fbd23f96ad2b5")]
public class UseAbilityOnBodyPartNodeElement : BehaviourTreeNodeElement<UseAbilityOnBodyPartNode>
{
	public AbilityVariableReference Ability;

	public EntityVariableReference Target;

	public BodyPartVariableReference BodyPart;

	protected override UseAbilityOnBodyPartNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		EntityVariable runtimeVariable = Target.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable2 = Ability.GetRuntimeVariable(blackboard);
		BodyPartVariable runtimeVariable3 = BodyPart.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new UseAbilityOnBodyPartNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, runtimeInternalDataVariable);
	}
}
