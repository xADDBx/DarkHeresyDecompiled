using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("1390271155944ca8838cab659c6d52b9")]
public class AbilityRuleTriggerTarget : AbilityTrigger, ITargetRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, ITargetRulebookSubscriber
{
	public bool AssignCasterAsTarget;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			RunAction(evt.Ability.Blueprint, evt.ConcreteInitiator, evt.AbilityTarget, AssignCasterAsTarget, this);
		}
	}
}
