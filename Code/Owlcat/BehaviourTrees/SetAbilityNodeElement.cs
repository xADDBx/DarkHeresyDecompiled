using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Ability/Set Ability", "Set Ability")]
[TypeId("9b9614737b214562b80e03bec402748a")]
public class SetAbilityNodeElement : BehaviourTreeNodeElement<SetAbilityNode>
{
	public AbilityVariableReference Variable;

	public AbilityVariableReference Value;

	protected override SetAbilityNode CreateTypedNode(Blackboard blackboard)
	{
		AbilityVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable2 = Value.GetRuntimeVariable(blackboard);
		return new SetAbilityNode(runtimeVariable, runtimeVariable2);
	}
}
