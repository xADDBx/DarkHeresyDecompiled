using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[Serializable]
[TypeId("4633613525bb42aeb9996511f056ce0b")]
public class HitChanceModifierTarget : HitChanceModifier, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
