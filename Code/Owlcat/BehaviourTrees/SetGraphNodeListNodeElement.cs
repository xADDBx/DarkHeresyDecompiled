using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList", "Set GraphNodeList")]
[TypeId("091551c8eb9940abb7b31155d13f9626")]
public class SetGraphNodeListNodeElement : BehaviourTreeNodeElement<SetGraphNodeListNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeListVariableReference NodeList;

	protected override SetGraphNodeListNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodeList.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListNode(runtimeVariable, runtimeVariable2);
	}
}
