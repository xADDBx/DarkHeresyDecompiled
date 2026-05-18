using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("Use DamageTriggerGlobal instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ee0460f02f7745b0a9894e1ebe22370d")]
public class GlobalDamageTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnAttacker;

	public ActionList ActionsOnTarget;

	public ActionList ActionsOnCaster;

	public ConditionsChecker ConditionsOnTarget;

	public ConditionsChecker ConditionsOnAttacker;

	public ConditionsChecker ConditionsOnCaster;

	public bool TriggersForDamageOverTime;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth == null || evt.HPBeforeDamage <= 0 || !m_Restrictions.IsPassed(base.Context, null, null, evt))
		{
			return;
		}
		BlueprintScriptableObject blueprintScriptableObject = evt.Reason.Context?.Blueprint;
		if ((blueprintScriptableObject is BlueprintBuff || blueprintScriptableObject is BlueprintAreaEffect) && !TriggersForDamageOverTime)
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
				flag2 = ConditionsOnAttacker.Check();
			}
			bool flag3;
			using (EvalContext.PushFact(base.Fact, target))
			{
				flag3 = ConditionsOnTarget.Check();
			}
			if (flag2 && flag && flag3)
			{
				base.Fact.RunActionInContext(ActionsOnCaster, maybeCaster);
				base.Fact.RunActionInContext(ActionsOnTarget, target);
				base.Fact.RunActionInContext(ActionsOnAttacker, initiator);
			}
		}
	}
}
