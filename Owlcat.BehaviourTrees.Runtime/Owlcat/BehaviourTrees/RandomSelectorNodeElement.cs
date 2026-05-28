using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Random Selector", "Random Selector")]
[TypeId("551f54b3997e4609bb1286eaf9f58918")]
public class RandomSelectorNodeElement : BehaviourTreeNodeElement<RandomSelectorNode>
{
	public List<int> Weights = new List<int>();

	protected override RandomSelectorNode CreateTypedNode(Blackboard blackboard)
	{
		return new RandomSelectorNode(Weights);
	}
}
