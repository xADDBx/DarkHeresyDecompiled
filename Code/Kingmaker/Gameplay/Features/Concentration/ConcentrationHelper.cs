using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Gameplay.Features.Concentration;

public static class ConcentrationHelper
{
	public static bool BreakConcentration(this BlueprintAbility blueprint)
	{
		return blueprint.IsGrenade;
	}

	public static bool BreakConcentration(this BlueprintAbilityWrapper blueprint)
	{
		return blueprint.IsGrenade;
	}
}
