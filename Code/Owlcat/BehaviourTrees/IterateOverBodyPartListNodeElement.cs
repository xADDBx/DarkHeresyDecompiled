using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.AI;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Iterations/Iterate over BodyPartList", "Iterate over BodyPartList")]
[TypeId("aabcc91ebd3748bda898707f9fc9daec")]
public class IterateOverBodyPartListNodeElement : BehaviourTreeNodeElement<IterateOverListNode<BpRef<BlueprintBodyPart>>>
{
	public BodyPartListVariableReference List;

	public BodyPartVariableReference Current;

	public IterationExitCondition ExitCondition;

	public bool RandomOrder;

	public bool UseSpecifiedNumberOfIterations;

	[ShowIf("UseSpecifiedNumberOfIterations")]
	public int NumberOfIterations;

	protected override IterateOverListNode<BpRef<BlueprintBodyPart>> CreateTypedNode(Blackboard blackboard)
	{
		BodyPartListVariable runtimeVariable = List.GetRuntimeVariable(blackboard);
		BodyPartVariable runtimeVariable2 = Current.GetRuntimeVariable(blackboard);
		return new IterateOverListNode<BpRef<BlueprintBodyPart>>(runtimeVariable, runtimeVariable2, ExitCondition, RandomOrder, UseSpecifiedNumberOfIterations ? NumberOfIterations : 0);
	}
}
