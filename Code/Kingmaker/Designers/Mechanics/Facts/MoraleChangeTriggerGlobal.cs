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
[ComponentName("Morale/MoraleChangeTriggerGlobal")]
[TypeId("a5d86a6f7a6f41efbe1a9b1555e72dca")]
public sealed class MoraleChangeTriggerGlobal : MoraleChangeTrigger, IMoraleChangeRawValueCalculatedTrigger, ISubscriber<IMechanicEntity>, ISubscriber, IGlobalRulebookHandler<RulePerformMoraleChange>, IRulebookHandler<RulePerformMoraleChange>, IGlobalRulebookSubscriber
{
	void IMoraleChangeRawValueCalculatedTrigger.HandleMoraleChangeRawValueCalculated(RulePerformMoraleChange rule)
	{
		TryTriggerBefore(rule);
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventAboutToTrigger(RulePerformMoraleChange evt)
	{
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventDidTrigger(RulePerformMoraleChange evt)
	{
		TryTriggerAfter(evt);
	}
}
