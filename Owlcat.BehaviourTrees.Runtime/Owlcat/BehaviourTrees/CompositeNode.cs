using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public abstract class CompositeNode : BehaviourTreeNode, IHasChildrenNode
{
	public List<BehaviourTreeNode> Children { get; } = new List<BehaviourTreeNode>();

}
