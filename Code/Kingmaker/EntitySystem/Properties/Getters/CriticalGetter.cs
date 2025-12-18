using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("13d4b63ae0754698a750e0631e10e189")]
public class CriticalGetter : IntPropertyGetter
{
	public CriticalParameterType CriticalParameterType;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return CriticalParameterType switch
		{
			CriticalParameterType.BonusCriticalHitChance => "Bonus Critical Hit Chance of " + FormulaTargetScope.Current, 
			CriticalParameterType.BonusCriticalDamage => "Bonus Critical Damage of " + FormulaTargetScope.Current, 
			_ => "", 
		};
	}
}
