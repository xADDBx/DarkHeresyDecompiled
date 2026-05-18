using JetBrains.Annotations;
using Kingmaker.EntitySystem.Properties;

namespace Owlcat.BehaviourTrees;

public class PropertyCalculatorBlueprintVariable : BlackboardVariable<PropertyCalculatorBlueprint>
{
	[CanBeNull]
	public override PropertyCalculatorBlueprint Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
