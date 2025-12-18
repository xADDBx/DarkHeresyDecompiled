using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode from Position", "Set GraphNode from Position")]
[TypeId("aa8212ad1647467898f7a389a3aca706")]
public class SetGraphNodeFromPositionNodeElement : BehaviourTreeNodeElement<SetGraphNodeFromPositionNode>
{
	public GraphNodeVariableReference Variable;

	public PositionVariableReference PositionVariable;

	protected override SetGraphNodeFromPositionNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		PositionVariable runtimeVariable2 = PositionVariable.GetRuntimeVariable(blackboard);
		return new SetGraphNodeFromPositionNode(runtimeVariable, runtimeVariable2);
	}
}
