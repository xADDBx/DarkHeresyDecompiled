using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace Kingmaker.Framework.Abilities;

public class AbilityExecutionHandler : IAbilityPipelineHandler
{
	public void OnProcessStart(AbilityExecutionContext context)
	{
		AbilityExecutionContext context = context;
		Game.Instance.GetController<PsychicPhenomenaController>()?.TryTriggerPsychicPhenomenaBeforeCast(context);
		EventBus.RaiseEvent((IMechanicEntity)context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
		{
			h.HandleExecutionProcessStart(context);
		}, isCheckRuntime: true);
	}

	public void PrepareCast(AbilityExecutionContext context)
	{
		AbilityExecutionContext context = context;
		context.Recalculate();
		context.AbilityBlueprint.CallComponents(delegate(IAbilityOnCastLogic c)
		{
			c.OnCast(context);
		});
	}

	public void OnProcessEnd(AbilityExecutionContext context)
	{
		AbilityExecutionContext context = context;
		Game.Instance.GetController<PsychicPhenomenaController>()?.TryTriggerPsychicPhenomenaAfterCast(context);
		EventBus.RaiseEvent((IMechanicEntity)context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
		{
			h.HandleExecutionProcessEnd(context);
		}, isCheckRuntime: true);
	}

	public void OnProcessCleanup(AbilityExecutionContext context)
	{
		AbilityExecutionContext context = context;
		context.AbilityBlueprint.CallComponents(delegate(AbilityCustomLogic c)
		{
			c.Cleanup(context);
		});
	}

	public void SpawnFxs(AbilityExecutionContext context, AbilitySpawnFxTime time, TargetWrapper? selectedTarget = null)
	{
		foreach (AbilitySpawnFx fxSpawner in context.FxSpawners)
		{
			if (fxSpawner.Time == time)
			{
				fxSpawner.Spawn(context, selectedTarget);
			}
		}
	}

	public IEnumerator<AbilityDeliveryTarget> DeliverTargets(AbilityDeliverEffect deliverEffect, AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		return deliverEffect.Deliver(context, clickedTarget);
	}

	public void InvokeHaloEffect(AbilityHaloEffect haloEffect, AbilityExecutionContext context)
	{
		haloEffect.Apply(context);
	}

	public IEnumerable<AbilityDeliveryTarget> GetAdditionalTargets(AbilityAdditionalTargets component, AbilityExecutionContext context)
	{
		return component.GetTargets(context);
	}

	public bool ShouldApplyToDeliveryTarget(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget)
	{
		if (context.Ability.Blueprint.GetComponent<AbilityEffectMissIsHit>() == null && deliveryTarget.AttackRule != null)
		{
			return deliveryTarget.AttackRule.ResultIsHit;
		}
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
		applyEffect?.Apply(context, target);
	}

	public void OnEffectApplied(AbilityExecutionContext context)
	{
		AbilityExecutionContext context = context;
		SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect);
		EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
		{
			h.OnAbilityEffectApplied(context);
		});
	}

	public void OnTryToApplyToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		AbilityExecutionContext context = context;
		AbilityDeliveryTarget target = target;
		EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
		{
			h.OnTryToApplyAbilityEffect(context, target);
		});
	}

	public void OnEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		AbilityExecutionContext context = context;
		AbilityDeliveryTarget target = target;
		SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect, target.Target);
		EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
		{
			h.OnAbilityEffectAppliedToTarget(context, target);
		});
	}
}
