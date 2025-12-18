using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[TypeId("19be5dcf539a404487f98692dc73d872")]
public class PropertyCalculatorBlueprint : BlueprintScriptableObject
{
	public int Add;

	public PropertyCalculator Value;

	public int GetValue(PropertyContext context)
	{
		return Value.GetValue(context) + Add;
	}
}
