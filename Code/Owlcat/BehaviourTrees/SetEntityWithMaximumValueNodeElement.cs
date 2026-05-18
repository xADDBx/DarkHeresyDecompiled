using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/MechanicEntity/Set Entity with Maximum Value", "Set Entity with Maximum Value")]
[TypeId("5cd51286f16649569b3edb938814600b")]
public class SetEntityWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetEntityWithMaximumValueNode>
{
	public EntityVariableReference Variable;

	public MechanicEntityListVariableReference Entities;

	[OptionalParameter]
	public AbilityVariableReference Ability;

	public PropertyCalculator FunctionToMaximize;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetEntityWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetEntityWithMaximumValueNode(blackboard.GetAgentVariable(), Variable.GetRuntimeVariable(blackboard), Entities.GetRuntimeVariable(blackboard), Ability.GetOptionalRuntimeVariable<AbilityVariable>(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), functionToMaximize: FunctionToMaximize);
	}
}
