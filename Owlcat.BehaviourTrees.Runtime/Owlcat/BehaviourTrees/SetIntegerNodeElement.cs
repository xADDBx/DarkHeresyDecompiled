using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Integer/Set Integer", "Set Integer")]
[TypeId("4dcff6318d8d48ee9a9796793753dbc5")]
public class SetIntegerNodeElement : BehaviourTreeNodeElement<SetIntegerNode>
{
	public IntegerVariableReference Variable;

	public int SetValue;

	protected override SetIntegerNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetIntegerNode(Variable.GetRuntimeVariable(blackboard), SetValue);
	}
}
