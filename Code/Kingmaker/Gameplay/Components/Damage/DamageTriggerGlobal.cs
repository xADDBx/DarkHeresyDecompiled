using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[ComponentName("Damage/DamageTriggerGlobal")]
[TypeId("6a25a3c380f1456793ab4aece6e9ccd0")]
public sealed class DamageTriggerGlobal : DamageTrigger, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage evt)
	{
		TryTrigger(evt, before: true);
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage evt)
	{
		TryTrigger(evt, before: false);
	}
}
