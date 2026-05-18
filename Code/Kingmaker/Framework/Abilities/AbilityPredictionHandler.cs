using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace Kingmaker.Framework.Abilities;

public class AbilityPredictionHandler : IAbilityPipelineHandler
{
	private readonly AbilityPredictionContext _predictionContext;

	public AbilityPredictionHandler(AbilityPredictionContext predictionContext)
	{
		_predictionContext = predictionContext;
	}

	public void OnProcessStart(AbilityExecutionContext context)
	{
	}

	public void PrepareCast(AbilityExecutionContext context)
	{
		context.Recalculate();
	}

	public void OnProcessEnd(AbilityExecutionContext context)
	{
	}

	public void OnProcessCleanup(AbilityExecutionContext context)
	{
	}

	public void SpawnFxs(AbilityExecutionContext context, AbilitySpawnFxTime time, TargetWrapper? selectedTarget = null)
	{
	}

	public IEnumerator<AbilityDeliveryTarget> DeliverTargets(AbilityDeliverEffect deliverEffect, AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		if (deliverEffect is IAbilityPrediction abilityPrediction)
		{
			abilityPrediction.CollectPrediction(_predictionContext);
		}
		foreach (AbilityDeliveryTarget deliveryTarget in _predictionContext.DeliveryTargets)
		{
			yield return deliveryTarget;
		}
	}

	public bool ShouldApplyToDeliveryTarget(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget)
	{
		return true;
	}

	public bool IsImmune(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		MechanicEntity entity = target.Target.Entity;
		if (entity != null)
		{
			PartAbilityActionsImmunity optional = entity.GetOptional<PartAbilityActionsImmunity>();
			if (optional != null)
			{
				return optional.IsImmune(context);
			}
		}
		return false;
	}

	public void InvokeApplyEffect(AbilityApplyEffect? applyEffect, AbilityExecutionContext context, TargetWrapper target)
	{
		IAbilityPrediction prediction = applyEffect as IAbilityPrediction;
		if (prediction == null)
		{
			return;
		}
		MechanicEntity entity = target.Entity;
		if (entity != null)
		{
			_predictionContext.WithTarget(entity, delegate
			{
				prediction.CollectPrediction(_predictionContext);
			});
		}
		else
		{
			prediction.CollectPrediction(_predictionContext);
		}
	}

	public void OnEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void InvokeHaloEffect(AbilityHaloEffect haloEffect, AbilityExecutionContext context)
	{
		if (haloEffect is IAbilityPrediction abilityPrediction)
		{
			abilityPrediction.CollectPrediction(_predictionContext);
		}
	}

	public IEnumerable<AbilityDeliveryTarget> GetAdditionalTargets(AbilityAdditionalTargets component, AbilityExecutionContext context)
	{
		return component.GetTargets(context);
	}
}
