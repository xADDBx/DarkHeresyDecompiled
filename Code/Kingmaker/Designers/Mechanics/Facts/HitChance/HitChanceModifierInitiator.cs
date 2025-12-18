using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[Serializable]
[TypeId("428580af48b84509b97be533f03c7759")]
public class HitChanceModifierInitiator : HitChanceModifier, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
