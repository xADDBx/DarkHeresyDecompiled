using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.Controllers;

public class OffensiveActionsController : IController, IApplyAbilityEffectHandler, ISubscriber, IAreaEffectEnterHandler, ISubscriber<IAreaEffectEntity>, IGlobalRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, IGlobalRulebookSubscriber
{
	private static bool IsSuitableEntity(MechanicEntity entity)
	{
		if (entity.IsInState && entity.IsInGame)
		{
			return !entity.IsDisposed;
		}
		return false;
	}

	private static void RaiseEvents(MechanicEntity initiator, MechanicEntity target)
	{
		if (!(initiator is BaseUnitEntity entity))
		{
			return;
		}
		BaseUnitEntity targetUnit = target as BaseUnitEntity;
		if (targetUnit != null && IsSuitableEntity(initiator) && IsSuitableEntity(target))
		{
			EventBus.RaiseEvent((IBaseUnitEntity)entity, (Action<IUnitMakeOffensiveActionHandler>)delegate(IUnitMakeOffensiveActionHandler h)
			{
				h.HandleUnitMakeOffensiveAction(targetUnit);
			}, isCheckRuntime: true);
		}
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (!context.AbilityBlueprint.NotOffensive && target.Target.Entity != null && (context.Caster.IsEnemy(target.Target.Entity) || target.Target.Entity.IsNeutral))
		{
			RaiseEvents(context.Caster, target.Target.Entity);
		}
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void HandleUnitEnterAreaEffect(MechanicEntity entity)
	{
		AreaEffectEntity entity2 = EventInvokerExtensions.GetEntity<AreaEffectEntity>();
		MechanicEntity maybeCaster = entity2.Context.MaybeCaster;
		if (maybeCaster is BaseUnitEntity initiator && entity.IsEnemy(maybeCaster) && entity2.AffectEnemies && entity2.AggroEnemies)
		{
			RaiseEvents(initiator, entity);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		AbilityData ability = evt.Ability;
		if (((object)ability == null || !ability.Blueprint.NotOffensive) && (evt.Initiator.IsEnemy(evt.Target) || evt.Target.IsNeutral))
		{
			RaiseEvents(evt.Initiator, evt.Target);
		}
	}
}
