using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public interface IHasChildrenNode
{
	List<BehaviourTreeNode> Children { get; }
}
