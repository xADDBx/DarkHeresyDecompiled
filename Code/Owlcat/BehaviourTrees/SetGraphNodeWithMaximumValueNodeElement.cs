using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode with Maximum Value", "Set GraphNode with Maximum Value")]
[TypeId("3d54cd5ce23d4a8b9c4d1fb00d7a4df3")]
public class SetGraphNodeWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetGraphNodeWithMaximumValueNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeListVariableReference NodesList;

	public MechanicEntityListVariableReference Entities;

	public PropertyCalculator FunctionToMaximize;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetGraphNodeWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetGraphNodeWithMaximumValueNode(blackboard.GetAgentVariable(), Variable.GetRuntimeVariable(blackboard), NodesList.GetRuntimeVariable(blackboard), Entities.GetRuntimeVariable(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), functionToMaximize: FunctionToMaximize);
	}
}
