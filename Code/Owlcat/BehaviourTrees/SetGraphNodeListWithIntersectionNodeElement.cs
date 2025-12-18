using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList with Intersection", "Set GraphNodeList with Intersection")]
[TypeId("2ea6f70e7b024af6850487b3f5e5d1cd")]
public class SetGraphNodeListWithIntersectionNodeElement : BehaviourTreeNodeElement<SetGraphNodeListWithIntersectionNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeListVariableReference NodeList1;

	public GraphNodeListVariableReference NodeList2;

	protected override SetGraphNodeListWithIntersectionNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodeList1.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable3 = NodeList2.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListWithIntersectionNode(runtimeVariable, runtimeVariable2, runtimeVariable3);
	}
}
