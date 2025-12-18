using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("4eb2e1f092e743dd8bcad0cceeaa5ed8")]
public class AbilityApplyEffectTriggerInitiator : AbilityTrigger, IApplyAbilityEffectHandler, ISubscriber
{
	[SerializeField]
	private bool AssignOwnerAsTarget;

	[SerializeField]
	private bool m_AssignContextFromAbility;

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (context.Caster == base.Owner && Restrictions.IsPassed(context, base.Owner, target.Target.Entity))
		{
			RunAction(context.Ability.Blueprint, context, target.Target, AssignOwnerAsTarget, m_AssignContextFromAbility);
		}
	}
}
