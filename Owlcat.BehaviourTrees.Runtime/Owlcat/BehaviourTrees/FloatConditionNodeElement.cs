using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Float/Float Condition Leaf", "Float Condition Leaf")]
[TypeId("a4ea3b3d541648018bdfbc6a1391cc70")]
public class FloatConditionNodeElement : ConditionNodeElement<FloatConditionNode>
{
	public FloatVariableReference LeftVariable;

	public FloatCompareOperator Operator;

	public FloatVariableReference RightVariable;

	protected override FloatConditionNode CreateTypedNode(Blackboard blackboard)
	{
		FloatVariable runtimeVariable = LeftVariable.GetRuntimeVariable(blackboard);
		FloatVariable runtimeVariable2 = RightVariable.GetRuntimeVariable(blackboard);
		return new FloatConditionNode(AbortType, runtimeVariable, runtimeVariable2, Operator);
	}
}
