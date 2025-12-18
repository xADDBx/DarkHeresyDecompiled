using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[ComponentName("Damage/DamageTriggerTarget")]
[TypeId("2a6944e09f50401b98aec4a5c89a9f03")]
public sealed class DamageTriggerTarget : DamageTrigger, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
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
