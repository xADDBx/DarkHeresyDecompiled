using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Distance Condition Pass Node", "Distance Condition Pass Node")]
[TypeId("6a30becdf296495ca3a71be66c8b0fa3")]
public class DistanceConditionPassNodeElement : ConditionPassNodeElement<DistanceConditionPassNode>
{
	public EntityVariableReference FromVariable;

	public EntityVariableReference ToVariable;

	public FloatCompareOperator Operator;

	public FloatVariableReference DistanceVariable;

	protected override DistanceConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = FromVariable.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = ToVariable.GetRuntimeVariable(blackboard);
		FloatVariable runtimeVariable3 = DistanceVariable.GetRuntimeVariable(blackboard);
		return new DistanceConditionPassNode(AbortType, runtimeVariable, runtimeVariable2, runtimeVariable3, Operator);
	}
}
