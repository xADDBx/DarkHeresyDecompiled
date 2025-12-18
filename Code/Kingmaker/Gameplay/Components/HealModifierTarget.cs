using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Heal/HealModifierTarget")]
[TypeId("5737a32b2cfc465395d2acfec86b1730")]
public sealed class HealModifierTarget : HealModifier, ITargetRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateHeal>.OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateHeal>.OnEventDidTrigger(RuleCalculateHeal evt)
	{
	}
}
