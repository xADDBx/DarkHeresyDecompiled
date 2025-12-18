using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Experience;

namespace Kingmaker.Gameplay.Features.Morale.Utility;

public static class PowerBalanceUtility
{
	private static MoraleRoot Settings => MoraleRoot.Instance;

	public static float CalculatePowerBalanceContribution(this BaseUnitEntity unit)
	{
		if (unit == null || unit.IsDeadOrUnconscious || !unit.IsInCombat)
		{
			return 0f;
		}
		UnitDifficultyType difficulty = (unit.IsPlayerFaction ? UnitDifficultyType.Elite : unit.Blueprint.DifficultyType);
		float healthFraction = (float)Settings.PowerBalanceHealthBase / 100f + unit.Health.HitPointsLeftFraction;
		return CalculatePowerBalanceContribution(difficulty, healthFraction, unit.Morale.Value, unit.Morale.PowerFactor);
	}

	public static float CalculatePowerBalanceContribution(this BlueprintUnit unit)
	{
		return CalculatePowerBalanceContribution(unit.DifficultyType, 1f, 0f, 1f);
	}

	private static float CalculatePowerBalanceContribution(UnitDifficultyType difficulty, float healthFraction, float morale, float factor)
	{
		return (float)Settings.GetUnitDifficulty(difficulty) * ((float)MoraleRoot.Instance.PowerBalanceMoraleBase + morale) * healthFraction * factor;
	}
}
