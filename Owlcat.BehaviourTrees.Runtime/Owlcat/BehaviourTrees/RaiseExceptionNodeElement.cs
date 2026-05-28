using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Debug/Raise Exception", "Raise Exception")]
[TypeId("4f4052bcfaca4c498d58e33f22f9a821")]
public class RaiseExceptionNodeElement : BehaviourTreeNodeElement<RaiseExceptionNode>
{
	public string Message;

	protected override RaiseExceptionNode CreateTypedNode(Blackboard blackboard)
	{
		return new RaiseExceptionNode(Message);
	}
}
