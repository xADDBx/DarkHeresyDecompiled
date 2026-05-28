using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Boolean/Boolean Condition Leaf", "Boolean Condition Leaf")]
[TypeId("d048db281e244c72a18cf604a7e38fca")]
public class BooleanConditionNodeElement : ConditionNodeElement<BooleanConditionNode>
{
	public BooleanVariableReference Variable;

	public bool Invert;

	protected override BooleanConditionNode CreateTypedNode(Blackboard blackboard)
	{
		BooleanVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		return new BooleanConditionNode(AbortType, runtimeVariable, Invert);
	}
}
