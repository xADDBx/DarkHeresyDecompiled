using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("9cb8cadd214341dbbc558d4097cdf57c")]
public class AbilityRuleTriggerInitiator : AbilityTrigger, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool AssignOwnerAsTarget;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			RunAction(evt.Ability.Blueprint, evt.ConcreteInitiator, evt.AbilityTarget, AssignOwnerAsTarget, this);
		}
	}
}
