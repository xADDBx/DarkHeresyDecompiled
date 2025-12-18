using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Ability Availability Condition Leaf", "Ability Availability Condition Leaf")]
[TypeId("4ce1e2559e684e42a29d8795926a5752")]
public class AbilityAvailabilityConditionNodeElement : ConditionNodeElement<AbilityAvailabilityConditionNode>
{
	public AbilityVariableReference Ability;

	public bool Invert;

	protected override AbilityAvailabilityConditionNode CreateTypedNode(Blackboard blackboard)
	{
		AbilityVariable runtimeVariable = Ability.GetRuntimeVariable(blackboard);
		return new AbilityAvailabilityConditionNode(AbortType, runtimeVariable, Invert);
	}
}
