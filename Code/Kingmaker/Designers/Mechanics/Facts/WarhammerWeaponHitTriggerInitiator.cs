using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67bd9d5235e74484bb1d673b885ab430")]
public class WarhammerWeaponHitTriggerInitiator : WarhammerWeaponHitTriggerBase, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool OnlyMelee;

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, null, null, evt))
			{
				return;
			}
		}
		CheckConditionsAndRunActions(evt.ConcreteInitiator, evt.ConcreteTarget, evt.RollPerformAttackRule.IsMelee, evt.ResultIsHit);
	}

	private void CheckConditionsAndRunActions(MechanicEntity initiator, MechanicEntity target, bool isMelee, bool isHit)
	{
		if (!OnlyMelee || isMelee)
		{
			TryRunActions(initiator, target, isHit);
		}
	}
}
