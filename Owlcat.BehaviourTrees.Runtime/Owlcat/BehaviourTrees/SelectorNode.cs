using System;

namespace Owlcat.BehaviourTrees;

public class SelectorNode : CompositeNode
{
	private int m_NextChildIndex;

	public override NodeVisitResult ForwardVisit()
	{
		m_NextChildIndex = 0;
		return TryVisitNextChild();
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		return result switch
		{
			NodeResult.Success => NodeVisitResult.Success, 
			NodeResult.Running => NodeVisitResult.Running, 
			NodeResult.Failure => TryVisitNextChild(), 
			_ => throw new Exception($"Unknown result: {result}"), 
		};
	}

	private NodeVisitResult TryVisitNextChild()
	{
		while (m_NextChildIndex < base.Children.Count)
		{
			BehaviourTreeNode behaviourTreeNode = base.Children[m_NextChildIndex];
			m_NextChildIndex++;
			if (!behaviourTreeNode.IsDisabled)
			{
				return NodeVisitResult.GoForward(behaviourTreeNode);
			}
		}
		return NodeVisitResult.Failure;
	}
}
