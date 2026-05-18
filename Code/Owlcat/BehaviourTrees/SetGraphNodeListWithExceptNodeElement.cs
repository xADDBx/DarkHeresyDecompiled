using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList with Except", "Set GraphNodeList with Except")]
[TypeId("7cce6dafea192c94e8b02fe024a2db36")]
public class SetGraphNodeListWithExceptNodeElement : BehaviourTreeNodeElement<SetGraphNodeListWithExceptNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeListVariableReference NodeList1;

	public GraphNodeListVariableReference NodeList2;

	protected override SetGraphNodeListWithExceptNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodeList1.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable3 = NodeList2.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListWithExceptNode(runtimeVariable, runtimeVariable2, runtimeVariable3);
	}
}
