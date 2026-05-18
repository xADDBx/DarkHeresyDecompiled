using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[ComponentName("Ability/AbilityLifecycleTriggerCaster")]
[TypeId("6ebdb034bf2f486b932f9342e175ec6d")]
public class AbilityLifecycleTriggerCaster : AbilityLifecycleTrigger, IAbilityExecutionProcessHandler<EntitySubscriber>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, EntitySubscriber>, IApplyAbilityEffectHandler
{
	private class AffectedTargetsData : IEntityFactComponentTransientData
	{
		public AbilityData ActiveAbility;

		public readonly HashSet<MechanicEntity> AffectedTargets = new HashSet<MechanicEntity>();
	}

	public ActionList ActionsOnAffectedTargets = new ActionList();

	public RestrictionCalculator TargetRestrictions = new RestrictionCalculator();

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		RunStartActions(context);
		if (ActionsOnAffectedTargets.HasActions && context.Caster == base.Owner && (!Condition.Any || CheckCondition(context)))
		{
			AffectedTargetsData affectedTargetsData = RequestTransientData<AffectedTargetsData>();
			affectedTargetsData.ActiveAbility = context.Ability;
			affectedTargetsData.AffectedTargets.Clear();
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		AffectedTargetsData affectedTargetsData = RequestTransientData<AffectedTargetsData>();
		if (affectedTargetsData.ActiveAbility != null)
		{
			foreach (MechanicEntity affectedTarget in affectedTargetsData.AffectedTargets)
			{
				base.Fact.RunActionInContext(ActionsOnAffectedTargets, affectedTarget);
			}
			affectedTargetsData.AffectedTargets.Clear();
			affectedTargetsData.ActiveAbility = null;
		}
		RunEndActions(context);
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		AffectedTargetsData affectedTargetsData = RequestTransientData<AffectedTargetsData>();
		if (!(affectedTargetsData.ActiveAbility == null) && context.Caster == base.Owner && !(context.Ability != affectedTargetsData.ActiveAbility))
		{
			MechanicEntity entity = target.Target.Entity;
			if (entity != null && (TargetRestrictions.Empty || TargetRestrictions.IsPassed(context, base.Owner, entity)))
			{
				affectedTargetsData.AffectedTargets.Add(entity);
			}
		}
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}
}
