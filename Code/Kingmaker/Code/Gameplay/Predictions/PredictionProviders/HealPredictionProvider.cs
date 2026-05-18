using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public class HealPredictionProvider : IPredictionProvider<UIHealPredictionData, UIPredictionContext>
{
	private static readonly string m_ProfilerScopeId = "HealPredictionProvider.Get)";

	public UIHealPredictionData Get(UIPredictionContext ctx)
	{
		using (ProfileScope.New(m_ProfilerScopeId))
		{
			if (!ctx.Ability.IsHeal)
			{
				return default(UIHealPredictionData);
			}
			return GetHeal(ctx.Ability, ctx.Target);
		}
	}

	private UIHealPredictionData GetHeal(AbilityData ability, MechanicEntity target)
	{
		HealPredictionData healPrediction = ability.GetHealPrediction(target);
		if (healPrediction == null)
		{
			return default(UIHealPredictionData);
		}
		UIHealPredictionData result = default(UIHealPredictionData);
		result.MinHeal = healPrediction.MinValue;
		result.MaxHeal = healPrediction.MaxValue;
		result.HealStrategy = healPrediction.HealStrategy;
		result.Bonus = healPrediction.Bonus;
		return result;
	}
}
