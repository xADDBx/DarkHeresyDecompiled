using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.Framework.Abilities;

public static class AbilityPrediction
{
	public static AbilityPredictionResult Predict(AbilityData ability, TargetWrapper clickedTarget)
	{
		if (!IsSupported(ability.Caster))
		{
			PFLog.Default.Warning("AbilityPrediction.Predict: unsupported caster type {0}; skipping prediction", ability.Caster?.GetType().Name ?? "<null>");
			return AbilityPredictionResult.Empty;
		}
		MechanicEntity entity = clickedTarget.Entity;
		if (entity != null && !IsSupported(entity))
		{
			PFLog.Default.Warning("AbilityPrediction.Predict: unsupported target type {0}; skipping prediction", entity.GetType().Name);
			return AbilityPredictionResult.Empty;
		}
		using AbilityPredictionContext abilityPredictionContext = new AbilityPredictionContext(ability, clickedTarget);
		AbilityPredictionHandler handler = new AbilityPredictionHandler(abilityPredictionContext);
		AbilityExecutionProcess abilityExecutionProcess = new AbilityExecutionProcess(abilityPredictionContext.ExecutionContext, handler);
		while (!abilityExecutionProcess.IsEnded)
		{
			abilityExecutionProcess.Tick();
		}
		return abilityPredictionContext.BuildResult();
	}

	private static bool IsSupported(MechanicEntity? entity)
	{
		if (!(entity is BaseUnitEntity))
		{
			return entity is DestructibleEntity;
		}
		return true;
	}
}
