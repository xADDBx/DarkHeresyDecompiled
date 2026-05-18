using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode to Use Ability From", "Set GraphNode to Use Ability From")]
[TypeId("115532e9ba5b4651a7a0e24f5ef6a58f")]
public class SetGraphNodeToUseAbilityFromNodeElement : BehaviourTreeNodeElement<SetGraphNodeToUseAbilityFromNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeListVariableReference NodesList;

	public MechanicEntityListVariableReference Targets;

	public AbilityVariableReference Ability;

	public PropertyCalculator FunctionToMaximize;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetGraphNodeToUseAbilityFromNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetGraphNodeToUseAbilityFromNode(blackboard.GetAgentVariable(), Variable.GetRuntimeVariable(blackboard), NodesList.GetRuntimeVariable(blackboard), Targets.GetRuntimeVariable(blackboard), Ability.GetRuntimeVariable(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), functionToMaximize: FunctionToMaximize);
	}
}
