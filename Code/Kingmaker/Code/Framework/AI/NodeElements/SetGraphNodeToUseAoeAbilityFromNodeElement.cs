using Kingmaker.Code.Framework.AI.Nodes;
using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Framework.AI.NodeElements;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode to Use AoE Ability From", "Set GraphNode to Use AoE Ability From")]
[TypeId("7efe83d4a0974b8890540078aba21a15")]
public class SetGraphNodeToUseAoeAbilityFromNodeElement : BehaviourTreeNodeElement<SetGraphNodeToUseAoeAbilityFromNode>
{
	public GraphNodeVariableReference AoeVariable;

	public GraphNodeVariableReference PositionVariable;

	public GraphNodeListVariableReference CasterNodeList;

	public GraphNodeListVariableReference NodesList;

	public AbilityVariableReference Ability;

	public int MinTotalValueToCastAbility;

	public PropertyCalculator TargetValueCalculator;

	public bool IncludeDeadUnitsInCalculations;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetGraphNodeToUseAoeAbilityFromNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetGraphNodeToUseAoeAbilityFromNode(blackboard.GetAgentVariable(), AoeVariable.GetRuntimeVariable(blackboard), PositionVariable.GetRuntimeVariable(blackboard), CasterNodeList.GetRuntimeVariable(blackboard), NodesList.GetRuntimeVariable(blackboard), Ability.GetRuntimeVariable(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), minTotalValueToCastAbility: MinTotalValueToCastAbility, targetValueCalculator: TargetValueCalculator, includeDeadUnitsInCalculations: IncludeDeadUnitsInCalculations);
	}
}
