using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Integer/Integer Condition Pass Node", "Integer Condition Pass Node")]
[TypeId("9069eae1e4ed43a79fcb1ffee6e9ab83")]
public class IntegerConditionPassNodeElement : ConditionPassNodeElement<IntegerConditionPassNode>
{
	public IntegerVariableReference LeftVariable;

	public IntegerCompareOperator Operator;

	public IntegerVariableReference RightVariable;

	protected override IntegerConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		IntegerVariable runtimeVariable = LeftVariable.GetRuntimeVariable(blackboard);
		IntegerVariable runtimeVariable2 = RightVariable.GetRuntimeVariable(blackboard);
		return new IntegerConditionPassNode(AbortType, runtimeVariable, runtimeVariable2, Operator);
	}
}
