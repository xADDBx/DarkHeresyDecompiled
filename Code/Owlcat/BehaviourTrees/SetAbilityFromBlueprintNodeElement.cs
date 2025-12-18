using Kingmaker.Blueprints;
using Owlcat.AI;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Ability/Set Ability From Blueprint", "Set Ability")]
[TypeId("42255c0533ee4f80a95c6dd99793ef60")]
public class SetAbilityFromBlueprintNodeElement : BehaviourTreeNodeElement<SetAbilityFromBlueprintNode>
{
	public AbilityVariableReference Variable;

	[ValidateNotNull]
	public BlueprintAbilityReference Blueprint;

	protected override SetAbilityFromBlueprintNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		AbilityVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		return new SetAbilityFromBlueprintNode(agentVariable, runtimeVariable, Blueprint.Get());
	}
}
