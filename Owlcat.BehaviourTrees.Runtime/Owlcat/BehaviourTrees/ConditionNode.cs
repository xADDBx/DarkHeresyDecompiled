namespace Owlcat.BehaviourTrees;

public abstract class ConditionNode : BehaviourTreeNode
{
	public AbortType AbortType { get; }

	public abstract bool IsPassed();

	protected ConditionNode(AbortType abortType)
	{
		AbortType = abortType;
	}

	public override NodeVisitResult ForwardVisit()
	{
		if (!IsPassed())
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}
}
