using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Selector", "Selector")]
[TypeId("62285927e8ed4275824e07b9fac671a2")]
public class SelectorNodeElement : BehaviourTreeNodeElement<SelectorNode>
{
	protected override SelectorNode CreateTypedNode(Blackboard blackboard)
	{
		return new SelectorNode();
	}
}
