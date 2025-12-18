using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities;

public static class BlueprintAbilityWrapperExtensions
{
	public static bool ContainsAbility(this IEnumerable<BlueprintAbility> searchingPool, BlueprintAbilityWrapper wrapper)
	{
		if (wrapper == null)
		{
			return false;
		}
		foreach (BlueprintAbility item in searchingPool)
		{
			if (wrapper.SameAbility(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsAbility(this IEnumerable<BlueprintAbilityReference> searchingPool, BlueprintAbilityWrapper wrapper)
	{
		if (wrapper == null)
		{
			return false;
		}
		foreach (BlueprintAbilityReference item in searchingPool)
		{
			if (wrapper.SameAbility(item))
			{
				return true;
			}
		}
		return false;
	}
}
