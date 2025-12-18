using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Iterations/Iterate over MechanicEntityList", "Iterate over MechanicEntityList")]
[TypeId("5fc82e1e63534f94b40fea01991102f8")]
public class IterateOverMechanicEntityListNodeElement : BehaviourTreeNodeElement<IterateOverListNode<MechanicEntity>>
{
	public MechanicEntityListVariableReference List;

	public EntityVariableReference Current;

	public IterationExitCondition ExitCondition;

	public bool RandomOrder;

	public bool UseSpecifiedNumberOfIterations;

	[ShowIf("UseSpecifiedNumberOfIterations")]
	public int NumberOfIterations;

	protected override IterateOverListNode<MechanicEntity> CreateTypedNode(Blackboard blackboard)
	{
		MechanicEntityListVariable runtimeVariable = List.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Current.GetRuntimeVariable(blackboard);
		return new IterateOverListNode<MechanicEntity>(runtimeVariable, runtimeVariable2, ExitCondition, RandomOrder, UseSpecifiedNumberOfIterations ? NumberOfIterations : 0);
	}
}
