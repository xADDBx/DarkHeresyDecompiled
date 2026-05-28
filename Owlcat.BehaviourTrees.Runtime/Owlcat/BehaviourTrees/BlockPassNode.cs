using System;

namespace Owlcat.BehaviourTrees;

public abstract class BlockPassNode : BehaviourTreeNode, IHasChildNode
{
	private readonly WhenBlockPassRule m_WhenBlockPassRule;

	private readonly ResultInBlockedStateRule m_ResultInBlockedStateRule;

	public BehaviourTreeNode Child { get; set; }

	public NodeResult LastResult { get; private set; }

	protected BlockPassNode(WhenBlockPassRule whenBlockPassRule, ResultInBlockedStateRule resultInBlockedStateRule)
	{
		m_WhenBlockPassRule = whenBlockPassRule;
		m_ResultInBlockedStateRule = resultInBlockedStateRule;
		LastResult = NodeResult.Failure;
	}

	public abstract bool IsStillBlocked();

	protected abstract void Block();

	public override NodeVisitResult ForwardVisit()
	{
		if (!IsStillBlocked())
		{
			return NodeVisitResult.GoForward(Child);
		}
		return NodeVisitResult.GoBackward(GetInBlockedStateResult());
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		if (result == NodeResult.Running)
		{
			return NodeVisitResult.Running;
		}
		LastResult = result;
		if (ShouldBlockAfterPass(result))
		{
			Block();
		}
		return NodeVisitResult.GoBackward(result);
	}

	private bool ShouldBlockAfterPass(NodeResult result)
	{
		return m_WhenBlockPassRule switch
		{
			WhenBlockPassRule.OnSuccess => result == NodeResult.Success, 
			WhenBlockPassRule.OnFailure => result == NodeResult.Failure, 
			WhenBlockPassRule.OnAnyPass => true, 
			_ => throw new Exception(string.Format("Unknown {0}: {1}", "WhenBlockPassRule", m_WhenBlockPassRule)), 
		};
	}

	private NodeResult GetInBlockedStateResult()
	{
		return m_ResultInBlockedStateRule switch
		{
			ResultInBlockedStateRule.LastResult => LastResult, 
			ResultInBlockedStateRule.Success => NodeResult.Success, 
			ResultInBlockedStateRule.Failure => NodeResult.Failure, 
			_ => throw new Exception($"Unknown cooldown result rule: {m_ResultInBlockedStateRule}"), 
		};
	}
}
