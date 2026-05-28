using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Inverter", "Inverter")]
[TypeId("ae3dec8d7b04406689ff476a41b8207d")]
public class InverterNodeElement : BehaviourTreeNodeElement<InverterNode>
{
	protected override InverterNode CreateTypedNode(Blackboard blackboard)
	{
		return new InverterNode();
	}
}
