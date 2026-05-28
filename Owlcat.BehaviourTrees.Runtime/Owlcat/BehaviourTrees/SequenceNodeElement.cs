using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Sequence", "Sequence")]
[TypeId("ae082473f1ba4e59ab6acfc5228df4b5")]
public class SequenceNodeElement : BehaviourTreeNodeElement<SequenceNode>
{
	protected override SequenceNode CreateTypedNode(Blackboard blackboard)
	{
		return new SequenceNode();
	}
}
