using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Ability Availability Condition Pass Node", "Ability Availability Condition Pass Node")]
[TypeId("9615e5bb8e10422394bfd6c230286367")]
public class AbilityAvailabilityConditionPassNodeElement : ConditionPassNodeElement<AbilityAvailabilityConditionPassNode>
{
	public AbilityVariableReference Ability;

	public bool Invert;

	protected override AbilityAvailabilityConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		AbilityVariable runtimeVariable = Ability.GetRuntimeVariable(blackboard);
		return new AbilityAvailabilityConditionPassNode(AbortType, runtimeVariable, Invert);
	}
}
