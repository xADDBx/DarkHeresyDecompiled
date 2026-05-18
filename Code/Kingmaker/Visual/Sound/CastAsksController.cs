using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class CastAsksController : BaseAsksController, IClickActionHandler, ISubscriber, IHealingHandler, IClickMechanicActionBarSlotHandler
{
	void IClickActionHandler.OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction)
	{
		using (EvalContext.PushAsksContext(ability.Caster, ability.Caster))
		{
			ability.Caster?.View.Asks?.CantDo.Schedule();
		}
	}

	public void OnAttackRequested(BaseUnitEntity unit, UnitEntityView target)
	{
	}

	void IClickActionHandler.OnMoveRequested(Vector3 target)
	{
	}

	void IClickActionHandler.OnCastRequested(AbilityData ability, TargetWrapper target)
	{
		ScheduleOrder(ability, target);
	}

	void IClickActionHandler.OnItemUseRequested(AbilityData ability, TargetWrapper target)
	{
		ScheduleOrder(ability, target);
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		ScheduleHeal(healDamage);
	}

	private static void ScheduleHeal(RuleHealDamage ruleHealDamage)
	{
		MechanicEntity concreteInitiator = ruleHealDamage.ConcreteInitiator;
		MechanicEntity concreteTarget = ruleHealDamage.ConcreteTarget;
		if (concreteInitiator != null && concreteTarget != null)
		{
			if (concreteInitiator == concreteTarget)
			{
				concreteInitiator.View.Asks?.BeingSupported.Schedule();
			}
			else if (concreteTarget.IsAlly(concreteInitiator) && (concreteInitiator.View == null || concreteInitiator.View.Asks == null || !concreteInitiator.View.Asks.SupportAnAlly.Schedule(is2D: false, HealAllyCallback)))
			{
				concreteTarget.View.Asks?.BeingSupported.Schedule();
			}
		}
	}

	private static void HealAllyCallback(AsksContext askContext)
	{
		MechanicEntity entity = askContext.Target.Entity;
		if (entity != null && entity.View != null && entity.View.Asks != null)
		{
			PartLifeState lifeStateOptional = entity.GetLifeStateOptional();
			if (lifeStateOptional != null && lifeStateOptional.IsConscious)
			{
				entity.View.Asks?.BeingSupported.Schedule(is2D: false, null, askContext);
			}
		}
	}

	void IClickMechanicActionBarSlotHandler.HandleClickMechanicActionBarSlot(MechanicActionBarSlot action)
	{
		if (action.IsPossibleActive || action.Unit == null || action.Unit.View == null || action.Unit.View.Asks == null)
		{
			return;
		}
		using (EvalContext.PushAsksContext(action.Unit, action.Unit))
		{
			if (!(action is MechanicActionBarSlotEmpty))
			{
				action.Unit.View.Asks.CantDo.Schedule();
			}
		}
	}

	private static void ScheduleOrder(AbilityData ability, TargetWrapper target)
	{
		if (ability.Caster == null || ability.Caster.View == null || ability.Caster.View.Asks == null)
		{
			return;
		}
		using AbilityExecutionContext context = ability.ClaimExecutionContext(ability.Caster);
		using (EvalContext.PushContext(context, ability.Caster))
		{
			AskWrapper order = ability.Caster.View.Asks.Order;
			switch (ability.Blueprint.AbilityTag)
			{
			case AbilityTag.Heal:
			case AbilityTag.StarshipShotAbility:
			case AbilityTag.StarshipUltimateAbility:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AbilityTag.None:
				order?.Schedule();
				break;
			}
		}
	}
}
