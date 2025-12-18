using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Heal/HealModifierInitiator")]
[TypeId("f7d56e9e3be44985af7ff69b13be6ccf")]
public sealed class HealModifierInitiator : HealModifier, IInitiatorRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateHeal>.OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateHeal>.OnEventDidTrigger(RuleCalculateHeal evt)
	{
	}
}
