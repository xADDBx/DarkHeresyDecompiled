using System;
using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public abstract class ConditionPassNode : ConditionNode, IHasChildrenNode
{
	public List<BehaviourTreeNode> Children { get; } = new List<BehaviourTreeNode>();


	protected ConditionPassNode(AbortType abortType)
		: base(abortType)
	{
	}

	public override NodeVisitResult ForwardVisit()
	{
		if (Children == null || Children.Count == 0 || Children.Count > 2)
		{
			throw new Exception(GetType().Name + " with title '" + base.Title + "' must have 1 or 2 children");
		}
		if (IsPassed())
		{
			return NodeVisitResult.GoForward(Children[0]);
		}
		if (Children.Count != 2)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.GoForward(Children[1]);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		return NodeVisitResult.GoBackward(result);
	}
}
