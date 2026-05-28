using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Debug/Write Log", "Write Log")]
[TypeId("4a3fc5ea79d240108e9d4ef4066e0351")]
public class WriteLogNodeElement : BehaviourTreeNodeElement<WriteLogNode>
{
	public string Message;

	protected override WriteLogNode CreateTypedNode(Blackboard blackboard)
	{
		return new WriteLogNode
		{
			Message = Message
		};
	}
}
