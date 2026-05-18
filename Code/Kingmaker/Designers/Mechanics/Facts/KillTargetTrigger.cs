using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b3c64e68baa2498bbe1de319d0259e3a")]
[ContextRoleForField("ActionsOnCaster", ContextField.Target, "buff caster", FallsBackTo = "Caster")]
[ContextRoleForField("ActionsOnKiller", ContextField.Target, "killer", FallsBackTo = "rule.Initiator")]
[ContextRoleForField("ActionsOnTarget", ContextField.Target, "killed unit", FallsBackTo = "rule.Target")]
public class KillTargetTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnKiller;

	public ActionList ActionsOnCaster;

	public ActionList ActionsOnTarget;

	public ConditionsChecker ConditionsOnTarget;

	public ConditionsChecker ConditionsOnKiller;

	public ConditionsChecker ConditionsOnCaster;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth == null || evt.HPBeforeDamage <= 0 || evt.TargetHealth.HitPointsLeft > 0 || !m_Restrictions.IsPassed(base.Context, null, null, evt))
		{
			return;
		}
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity initiator = evt.Initiator;
		MechanicEntity target = evt.Target;
		if (maybeCaster != null && initiator != null && target != null)
		{
			bool flag;
			using (EvalContext.PushFact(base.Fact, maybeCaster))
			{
				flag = ConditionsOnCaster.Check();
			}
			bool flag2;
			using (EvalContext.PushFact(base.Fact, initiator))
			{
				flag2 = ConditionsOnKiller.Check();
			}
			bool flag3;
			using (EvalContext.PushFact(base.Fact, target))
			{
				flag3 = ConditionsOnTarget.Check();
			}
			if (flag2 && flag && flag3)
			{
				base.Fact.RunActionInContext(ActionsOnCaster, maybeCaster);
				base.Fact.RunActionInContext(ActionsOnKiller, initiator);
				base.Fact.RunActionInContext(ActionsOnTarget, target);
			}
		}
	}
}
