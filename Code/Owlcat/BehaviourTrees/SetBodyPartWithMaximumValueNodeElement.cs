using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/BodyPart/Set BodyPart with Maximum Value", "Set BodyPart with Maximum Value")]
[TypeId("4f539bbbd516447a98d84082d237499f")]
public class SetBodyPartWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetBodyPartWithMaximumValueNode>
{
	public BodyPartVariableReference Variable;

	public BodyPartListVariableReference BodyPartsList;

	public EntityVariableReference Target;

	public PropertyCalculator FunctionToMaximize;

	public int MinThresholdValue;

	[OptionalParameter]
	public PropertyCalculatorBlueprintVariableReference CalculatorBlueprint;

	protected override SetBodyPartWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		return new SetBodyPartWithMaximumValueNode(blackboard.GetAgentVariable(), Variable.GetRuntimeVariable(blackboard), BodyPartsList.GetRuntimeVariable(blackboard), Target.GetRuntimeVariable(blackboard), calculatorBlueprint: CalculatorBlueprint.GetOptionalRuntimeVariable<PropertyCalculatorBlueprintVariable>(blackboard), functionToMaximize: FunctionToMaximize, minThresholdValue: MinThresholdValue);
	}
}
