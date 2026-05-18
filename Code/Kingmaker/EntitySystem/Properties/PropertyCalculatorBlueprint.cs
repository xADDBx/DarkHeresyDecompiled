using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[TypeId("19be5dcf539a404487f98692dc73d872")]
public class PropertyCalculatorBlueprint : BlueprintScriptableObject
{
	public int Add;

	public PropertyCalculator Value;

	public int GetValue(MechanicEntity entity, IEvalContext context = null)
	{
		return Value.GetValue(entity, context) + Add;
	}
}
