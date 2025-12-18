using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoraleChangeTriggerInitiator")]
[TypeId("de7cd878287540bba03f853c42a3eded")]
public sealed class MoraleChangeTriggerInitiator : MoraleChangeTrigger, IMoraleChangeRawValueCalculatedTrigger, ISubscriber<IMechanicEntity>, ISubscriber, IInitiatorRulebookHandler<RulePerformMoraleChange>, IRulebookHandler<RulePerformMoraleChange>, IInitiatorRulebookSubscriber
{
	void IMoraleChangeRawValueCalculatedTrigger.HandleMoraleChangeRawValueCalculated(RulePerformMoraleChange rule)
	{
		if (rule.Initiator == base.Owner)
		{
			TryTriggerBefore(rule);
		}
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventAboutToTrigger(RulePerformMoraleChange evt)
	{
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventDidTrigger(RulePerformMoraleChange evt)
	{
		TryTriggerAfter(evt);
	}
}
