using Kingmaker.Utility.Attributes;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Iterations/Iterate over PositionList", "Iterate over PositionList")]
[TypeId("9e6a79403f4a44bb8550c27ffd3d9466")]
public class IterateOverPositionListNodeElement : BehaviourTreeNodeElement<IterateOverListNode<Vector3>>
{
	public PositionListVariableReference List;

	public PositionVariableReference Current;

	public IterationExitCondition ExitCondition;

	public bool RandomOrder;

	public bool UseSpecifiedNumberOfIterations;

	[ShowIf("UseSpecifiedNumberOfIterations")]
	public int NumberOfIterations;

	protected override IterateOverListNode<Vector3> CreateTypedNode(Blackboard blackboard)
	{
		PositionListVariable runtimeVariable = List.GetRuntimeVariable(blackboard);
		PositionVariable runtimeVariable2 = Current.GetRuntimeVariable(blackboard);
		return new IterateOverListNode<Vector3>(runtimeVariable, runtimeVariable2, ExitCondition, RandomOrder, UseSpecifiedNumberOfIterations ? NumberOfIterations : 0);
	}
}
