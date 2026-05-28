namespace Owlcat.BehaviourTrees;

public class WriteLogNode : ActionNode
{
	public string Message;

	protected override void DoAction()
	{
		BTLog.Log(Message);
	}
}
