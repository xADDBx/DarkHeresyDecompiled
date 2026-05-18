using System;
using Kingmaker.EntitySystem.Properties;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class PropertyCalculatorBlueprintVariableReference : VariableReference<PropertyCalculatorBlueprint>
{
	public PropertyCalculatorBlueprintVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<PropertyCalculatorBlueprintVariable>(Id);
	}
}
