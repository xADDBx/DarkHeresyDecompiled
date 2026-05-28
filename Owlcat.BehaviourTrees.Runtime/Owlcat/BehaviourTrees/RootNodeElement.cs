using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[TypeId("edf5230bd10145c69cbb7213b3a0995b")]
public class RootNodeElement : BehaviourTreeNodeElement<RootNode>
{
	protected override RootNode CreateTypedNode(Blackboard blackboard)
	{
		return new RootNode();
	}
}
