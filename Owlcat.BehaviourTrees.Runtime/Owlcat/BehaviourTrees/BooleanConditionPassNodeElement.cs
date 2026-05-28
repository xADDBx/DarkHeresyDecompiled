using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Boolean/Boolean Condition Pass Node", "Boolean Condition Pass Node")]
[TypeId("d0270e8a15224518aaa6155f93257717")]
public class BooleanConditionPassNodeElement : ConditionPassNodeElement<BooleanConditionPassNode>
{
	public BooleanVariableReference Variable;

	public bool Invert;

	protected override BooleanConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		BooleanVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		return new BooleanConditionPassNode(AbortType, runtimeVariable, Invert);
	}
}
