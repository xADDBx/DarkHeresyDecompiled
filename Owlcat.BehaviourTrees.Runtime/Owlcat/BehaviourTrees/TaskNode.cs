namespace Owlcat.BehaviourTrees;

public abstract class TaskNode : BehaviourTreeNode
{
	protected virtual NodeResult OnEnter()
	{
		return NodeResult.Running;
	}

	protected virtual void OnExit()
	{
	}

	protected abstract NodeResult OnRunningTick();

	protected virtual void OnAbort()
	{
	}

	public override NodeVisitResult ForwardVisit()
	{
		return NodeVisitResult.GoBackward(OnEnter());
	}

	public NodeResult RunningVisit()
	{
		NodeResult num = OnRunningTick();
		if (num != 0)
		{
			OnExit();
		}
		return num;
	}

	public void Abort()
	{
		OnAbort();
	}
}
