using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public static class PredictionHackContextExtensions
{
	public static int GetVeilDelta(this PredictionHackContext ctx)
	{
		return ctx?.VeilDeltaBeforeCast ?? 0;
	}

	public static int ModifyStat(this PredictionHackContext ctx, StatType statType, int statValue)
	{
		if (ctx != null && ctx.HasStatModifiers)
		{
			IReadOnlyDictionary<StatType, CompositeModifiersManager> statModifiers = ctx.StatModifiers;
			if (statModifiers != null && statModifiers.TryGetValue(statType, out var value))
			{
				return Math.Max((statType.IsAttribute() || statType == StatType.MaxHitPoints) ? 1 : int.MinValue, value.Apply(statValue));
			}
		}
		return statValue;
	}

	public static int GetPredictedRank([CanBeNull] this PredictionHackContext ctx, BlueprintFact fact)
	{
		if (ctx != null && ctx.HasFactRanksToIncrement)
		{
			List<BlueprintFact> factRanksToIncrement = ctx.FactRanksToIncrement;
			if (factRanksToIncrement != null)
			{
				int num = 0;
				for (int num2 = factRanksToIncrement.Count - 1; num2 >= 0; num2--)
				{
					if (factRanksToIncrement[num2] == fact)
					{
						factRanksToIncrement.RemoveAt(num2);
						num++;
					}
				}
				return num;
			}
		}
		return 0;
	}
}
