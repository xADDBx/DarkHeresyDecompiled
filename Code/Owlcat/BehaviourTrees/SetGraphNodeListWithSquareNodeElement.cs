using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList with Square", "Set GraphNodeList with Square")]
[TypeId("4eb370a8481243459b138d359dbb02e2")]
public class SetGraphNodeListWithSquareNodeElement : BehaviourTreeNodeElement<SetGraphNodeListWithSquareNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeVariableReference Center;

	[Range(1f, 20f)]
	public int SquareSide;

	protected override SetGraphNodeListWithSquareNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeVariable runtimeVariable2 = Center.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListWithSquareNode(runtimeVariable, runtimeVariable2, SquareSide);
	}
}
