using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Float/Increase Float", "Increase Float")]
[TypeId("7e01297125924e53a2824ee2333c7c09")]
public class IncreaseFloatNodeElement : BehaviourTreeNodeElement<IncreaseFloatNode>
{
	public FloatVariableReference Variable;

	public FloatVariableReference IncreaseValue;

	protected override IncreaseFloatNode CreateTypedNode(Blackboard blackboard)
	{
		FloatVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		FloatVariable runtimeVariable2 = IncreaseValue.GetRuntimeVariable(blackboard);
		return new IncreaseFloatNode(runtimeVariable, runtimeVariable2);
	}
}
