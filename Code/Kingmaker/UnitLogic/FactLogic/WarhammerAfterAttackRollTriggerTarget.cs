using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c19cadfe75cd6fc44b3f50e7fa124a26")]
public class WarhammerAfterAttackRollTriggerTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList Actions;

	public bool triggerAfterAttack;

	public bool onlyMeleeAttack;

	public bool ActionsOnTarget;

	public bool OnlyHit;

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
		TryToTrigger(evt, !triggerAfterAttack);
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		TryToTrigger(evt, triggerAfterAttack);
	}

	private void TryToTrigger(RulePerformAttack evt, bool afterAttackTrigger)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, null, null, evt))
			{
				return;
			}
		}
		if ((!onlyMeleeAttack || evt.IsMelee) && afterAttackTrigger && (!OnlyHit || evt.ResultIsHit))
		{
			base.Fact.RunActionInContext(Actions, (!ActionsOnTarget) ? evt.ConcreteInitiator.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
			base.ExecutesCount++;
		}
	}
}
