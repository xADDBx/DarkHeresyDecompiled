using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList with Circle", "Set GraphNodeList with Circle")]
[TypeId("d29436f29b81424d91a71b092dadae3b")]
public class SetGraphNodeListWithCircleNodeElement : BehaviourTreeNodeElement<SetGraphNodeListWithCircleNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeVariableReference Center;

	[Range(0f, 20f)]
	public int Radius;

	protected override SetGraphNodeListWithCircleNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeVariable runtimeVariable2 = Center.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListWithCircleNode(runtimeVariable, runtimeVariable2, Radius);
	}
}
