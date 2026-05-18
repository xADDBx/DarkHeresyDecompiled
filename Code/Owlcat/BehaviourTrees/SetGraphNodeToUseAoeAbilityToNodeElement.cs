using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode to Use AoE Ability To", "Set GraphNode to Use AoE Ability To")]
[TypeId("35c43790034c4da59c78d078820f99eb")]
public class SetGraphNodeToUseAoeAbilityToNodeElement : BehaviourTreeNodeElement<SetGraphNodeToUseAoeAbilityToNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeVariableReference CasterNode;

	public GraphNodeListVariableReference NodesList;

	public AbilityVariableReference Ability;

	public int MinTotalValueToCastAbility;

	public PropertyCalculator TargetValueCalculator;

	public bool IncludeDeadUnitsInCalculations;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetGraphNodeToUseAoeAbilityToNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetGraphNodeToUseAoeAbilityToNode(blackboard.GetAgentVariable(), Variable.GetRuntimeVariable(blackboard), CasterNode.GetRuntimeVariable(blackboard), NodesList.GetRuntimeVariable(blackboard), Ability.GetRuntimeVariable(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), minTotalValueToCastAbility: MinTotalValueToCastAbility, targetValueCalculator: TargetValueCalculator, includeDeadUnitsInCalculations: IncludeDeadUnitsInCalculations);
	}
}
