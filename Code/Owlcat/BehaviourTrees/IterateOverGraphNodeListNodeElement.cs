using Kingmaker.Utility.Attributes;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Iterations/Iterate over GraphNodeList", "Iterate over GraphNodeList")]
[TypeId("9631040e3a7743d8a1a0d692be150945")]
public class IterateOverGraphNodeListNodeElement : BehaviourTreeNodeElement<IterateOverListNode<GraphNode>>
{
	public GraphNodeListVariableReference List;

	public GraphNodeVariableReference Current;

	public IterationExitCondition ExitCondition;

	public bool RandomOrder;

	public bool UseSpecifiedNumberOfIterations;

	[ShowIf("UseSpecifiedNumberOfIterations")]
	public int NumberOfIterations;

	protected override IterateOverListNode<GraphNode> CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeListVariable runtimeVariable = List.GetRuntimeVariable(blackboard);
		GraphNodeVariable runtimeVariable2 = Current.GetRuntimeVariable(blackboard);
		return new IterateOverListNode<GraphNode>(runtimeVariable, runtimeVariable2, ExitCondition, RandomOrder, UseSpecifiedNumberOfIterations ? NumberOfIterations : 0);
	}
}
