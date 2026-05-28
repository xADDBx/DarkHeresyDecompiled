using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Boolean/Set Boolean", "Set Boolean")]
[TypeId("ca1e4d8c28684ebe8c43cc866e43634e")]
public class SetBooleanNodeElement : BehaviourTreeNodeElement<SetBooleanNode>
{
	public BooleanVariableReference Variable;

	public bool SetValue;

	protected override SetBooleanNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetBooleanNode(Variable.GetRuntimeVariable(blackboard), SetValue);
	}
}
