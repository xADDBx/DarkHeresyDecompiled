using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Fixed Result", "Fixed Result")]
[TypeId("a2c45b9a1a3748708c08497b9484bf74")]
public class FixedResultNodeElement : BehaviourTreeNodeElement<FixedResultNode>
{
	public NodeFixedResult Result;

	protected override FixedResultNode CreateTypedNode(Blackboard blackboard)
	{
		return new FixedResultNode(Result);
	}
}
