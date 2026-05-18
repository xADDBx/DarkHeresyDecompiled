using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public class MoralePredictionProvider : IPredictionProvider<UIMoralePredictionData, UIPredictionContext>
{
	private static readonly string m_ProfilerScopeId = "MoralePredictionProvider.Get)";

	public UIMoralePredictionData Get(UIPredictionContext ctx)
	{
		using (ProfileScope.New(m_ProfilerScopeId))
		{
			return GetMorale(ctx.Ability, ctx.CasterPosition, ctx.Target);
		}
	}

	private UIMoralePredictionData GetMorale(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		MoralePredictionRange moralePrediction = ability.GetMoralePrediction(target, casterPosition);
		UIMoralePredictionData result = default(UIMoralePredictionData);
		result.MinDelta = moralePrediction.MinDelta;
		result.MaxDelta = moralePrediction.MaxDelta;
		return result;
	}
}
