namespace Owlcat.BehaviourTrees;

public abstract class ActionNode : BehaviourTreeNode
{
	protected abstract void DoAction();

	public override NodeVisitResult ForwardVisit()
	{
		DoAction();
		return NodeVisitResult.Success;
	}
}
