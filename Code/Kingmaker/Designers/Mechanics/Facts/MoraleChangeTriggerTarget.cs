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
[ComponentName("Morale/MoraleChangeTriggerTarget")]
[TypeId("b44053d1916045d4a7ffd2d36f919d57")]
public sealed class MoraleChangeTriggerTarget : MoraleChangeTrigger, IMoraleChangeRawValueCalculatedTrigger<EntitySubscriber>, IMoraleChangeRawValueCalculatedTrigger, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IMoraleChangeRawValueCalculatedTrigger, EntitySubscriber>, ITargetRulebookHandler<RulePerformMoraleChange>, IRulebookHandler<RulePerformMoraleChange>, ITargetRulebookSubscriber
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
