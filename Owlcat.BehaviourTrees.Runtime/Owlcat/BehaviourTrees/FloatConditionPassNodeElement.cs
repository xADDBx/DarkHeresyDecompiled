using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Float/Float Condition Pass Node", "Float Condition Pass Node")]
[TypeId("a5aa3531c1aa47f48affebd8f4f545ec")]
public class FloatConditionPassNodeElement : ConditionPassNodeElement<FloatConditionPassNode>
{
	public FloatVariableReference LeftVariable;

	public FloatCompareOperator Operator;

	public FloatVariableReference RightVariable;

	protected override FloatConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		FloatVariable runtimeVariable = LeftVariable.GetRuntimeVariable(blackboard);
		FloatVariable runtimeVariable2 = RightVariable.GetRuntimeVariable(blackboard);
		return new FloatConditionPassNode(AbortType, runtimeVariable, runtimeVariable2, Operator);
	}
}
