using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("97f0ad03ca55ccb4692cfc80b7e626ea")]
public class NamedParameterPosition : PositionEvaluator
{
	public string Parameter;

	protected override Vector3 GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return Vector3.zero;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find position {0} in context parameters", Parameter);
		}
		if (value != null)
		{
			object value2 = value.Value;
			if (value2 is UnitReference unitReference)
			{
				return unitReference.Get()?.Position ?? Vector3.zero;
			}
			if (value2 is Entity entity)
			{
				return entity.Position;
			}
			if (value2 is Vector3)
			{
				return (Vector3)value2;
			}
		}
		Element.LogError(this, "WTF");
		return Vector3.zero;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
