using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Wait", "Wait")]
[TypeId("6be4e5402af84fe6aa3a245f5dc3266d")]
public class WaitNodeElement : BehaviourTreeNodeElement<WaitNode>
{
	public FloatVariableReference SecondsVariable;

	protected override WaitNode CreateTypedNode(Blackboard blackboard)
	{
		return new WaitNode(SecondsVariable.GetRuntimeVariable(blackboard));
	}
}
