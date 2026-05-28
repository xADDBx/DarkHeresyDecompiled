using System;

namespace Owlcat.BehaviourTrees;

public class SequenceNode : CompositeNode
{
	private int m_NextChildIndex;

	public override NodeVisitResult ForwardVisit()
	{
		if (base.Children.Count == 0)
		{
			return NodeVisitResult.Failure;
		}
		m_NextChildIndex = 1;
		return NodeVisitResult.GoForward(base.Children[0]);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		switch (result)
		{
		case NodeResult.Failure:
			return NodeVisitResult.Failure;
		case NodeResult.Running:
			return NodeVisitResult.Running;
		case NodeResult.Success:
		{
			if (m_NextChildIndex >= base.Children.Count)
			{
				return NodeVisitResult.Success;
			}
			BehaviourTreeNode childNode = base.Children[m_NextChildIndex];
			m_NextChildIndex++;
			return NodeVisitResult.GoForward(childNode);
		}
		default:
			throw new Exception($"Unknown result: {result}");
		}
	}
}
