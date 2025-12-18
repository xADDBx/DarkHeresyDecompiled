using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Distance Condition Leaf", "Distance Condition Leaf")]
[TypeId("1c717d626594489d9c70ca189833e546")]
public class DistanceConditionNodeElement : ConditionNodeElement<DistanceConditionNode>
{
	public EntityVariableReference FromVariable;

	public EntityVariableReference ToVariable;

	public FloatCompareOperator Operator;

	public FloatVariableReference DistanceVariable;

	protected override DistanceConditionNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = FromVariable.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = ToVariable.GetRuntimeVariable(blackboard);
		FloatVariable runtimeVariable3 = DistanceVariable.GetRuntimeVariable(blackboard);
		return new DistanceConditionNode(AbortType, runtimeVariable, runtimeVariable2, runtimeVariable3, Operator);
	}
}
