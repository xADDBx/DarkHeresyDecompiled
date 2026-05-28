using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Integer/Increase Integer", "Increase Integer")]
[TypeId("e9b1f0f73e7340eeae66bb23c1c576aa")]
public class IncreaseIntegerNodeElement : BehaviourTreeNodeElement<IncreaseIntegerNode>
{
	public IntegerVariableReference Variable;

	public IntegerVariableReference IncreaseVariable;

	protected override IncreaseIntegerNode CreateTypedNode(Blackboard blackboard)
	{
		IntegerVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		IntegerVariable runtimeVariable2 = IncreaseVariable.GetRuntimeVariable(blackboard);
		return new IncreaseIntegerNode(runtimeVariable, runtimeVariable2);
	}
}
