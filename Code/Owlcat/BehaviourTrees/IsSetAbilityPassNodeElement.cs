using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Ability/Is Set Ability Pass Node", "Is Set Ability Pass Node")]
[TypeId("83c9145f3d209f745be1c4721aa0763f")]
public class IsSetAbilityPassNodeElement : ConditionPassNodeElement<IsSetAbilityPassNode>
{
	public AbilityVariableReference AbilityVariable;

	public bool Invert;

	protected override IsSetAbilityPassNode CreateTypedNode(Blackboard blackboard)
	{
		AbilityVariable runtimeVariable = AbilityVariable.GetRuntimeVariable(blackboard);
		return new IsSetAbilityPassNode(AbortType, runtimeVariable, Invert);
	}
}
