namespace Owlcat.BehaviourTrees;

public class RepeatNode : BehaviourTreeNode, IHasChildNode
{
	private int m_CurrentRepeatTry;

	private readonly int m_RepeatCount;

	public int CurrentRepeatTry => m_CurrentRepeatTry;

	public BehaviourTreeNode Child { get; set; }

	public RepeatNode(int repeatCount)
	{
		m_RepeatCount = repeatCount;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_CurrentRepeatTry = 1;
		return NodeVisitResult.GoForward(Child);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		if (result == NodeResult.Running)
		{
			return NodeVisitResult.Running;
		}
		if (m_CurrentRepeatTry < m_RepeatCount)
		{
			m_CurrentRepeatTry++;
			return NodeVisitResult.GoForward(Child);
		}
		return NodeVisitResult.GoBackward(result);
	}
}
