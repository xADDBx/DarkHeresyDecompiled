using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Ability/Is Set Ability Leaf", "Is Set Ability")]
[TypeId("e9765d4a0d916f64c8338becec654296")]
public class IsSetAbilityNodeElement : ConditionNodeElement<IsSetAbilityNode>
{
	public AbilityVariableReference AbilityVariable;

	public bool Invert;

	protected override IsSetAbilityNode CreateTypedNode(Blackboard blackboard)
	{
		AbilityVariable runtimeVariable = AbilityVariable.GetRuntimeVariable(blackboard);
		return new IsSetAbilityNode(AbortType, runtimeVariable, Invert);
	}
}
