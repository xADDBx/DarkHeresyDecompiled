using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("54a86f92b75a4fda91624eecaebe6ad2")]
public class AbilityApplyEffectTriggerTarget : AbilityTrigger, IApplyAbilityEffectHandler, ISubscriber
{
	[SerializeField]
	private bool AssignCasterAsTarget;

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
		if (!(target.Target != base.Owner) && Restrictions.IsPassed(context, base.Owner, target.Target.Entity))
		{
			RunAction(context.Ability.Blueprint, context, target.Target, AssignCasterAsTarget, m_AssignContextFromAbility);
		}
	}
}
