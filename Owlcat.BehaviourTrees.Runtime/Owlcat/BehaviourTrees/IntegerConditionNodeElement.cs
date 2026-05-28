using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Integer/Integer Condition Leaf", "Integer Condition Leaf")]
[TypeId("f8c92720a9b94f10839cf7b6b35fd291")]
public class IntegerConditionNodeElement : ConditionNodeElement<IntegerConditionNode>
{
	public IntegerVariableReference LeftVariable;

	public IntegerCompareOperator Operator;

	public IntegerVariableReference RightVariable;

	protected override IntegerConditionNode CreateTypedNode(Blackboard blackboard)
	{
		IntegerVariable runtimeVariable = LeftVariable.GetRuntimeVariable(blackboard);
		IntegerVariable runtimeVariable2 = RightVariable.GetRuntimeVariable(blackboard);
		return new IntegerConditionNode(AbortType, runtimeVariable, runtimeVariable2, Operator);
	}
}
