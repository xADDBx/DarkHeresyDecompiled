using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Repeat", "Repeat")]
[TypeId("a731487fca98440eaaa2d2d3c17bb892")]
public class RepeatNodeElement : BehaviourTreeNodeElement<RepeatNode>
{
	public int RepeatCount;

	protected override RepeatNode CreateTypedNode(Blackboard blackboard)
	{
		return new RepeatNode(RepeatCount);
	}
}
