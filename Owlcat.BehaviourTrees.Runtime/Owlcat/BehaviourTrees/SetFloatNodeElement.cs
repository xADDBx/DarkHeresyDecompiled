using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Float/Set Float", "Set Float")]
[TypeId("6a7333f8ccc7483b907ad4b66d90ddf1")]
public class SetFloatNodeElement : BehaviourTreeNodeElement<SetFloatNode>
{
	public FloatVariableReference Variable;

	public float SetValue;

	protected override SetFloatNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetFloatNode(Variable.GetRuntimeVariable(blackboard), SetValue);
	}
}
