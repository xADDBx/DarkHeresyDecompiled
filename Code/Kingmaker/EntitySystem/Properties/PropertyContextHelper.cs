using System;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyContextHelper
{
	public static string ToShortString(this PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => "CE", 
			PropertyTargetType.CurrentTarget => "CT", 
			PropertyTargetType.ContextCaster => "CC", 
			PropertyTargetType.ContextOwner => "CO", 
			PropertyTargetType.ContextMainTarget => "CMT", 
			PropertyTargetType.RuleInitiator => "RI", 
			PropertyTargetType.RuleTarget => "RT", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static string Colorized(this PropertyTargetType type)
	{
		if (type == PropertyTargetType.CurrentEntity)
		{
			type = FormulaTargetScope.CurrentTarget;
		}
		if (FormulaTargetScope.NeedColorization)
		{
			return $"<color='green'>{type}</color>";
		}
		return type.ToString();
	}
}
