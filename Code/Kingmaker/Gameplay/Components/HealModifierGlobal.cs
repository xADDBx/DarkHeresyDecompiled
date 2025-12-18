using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Heal/HealModifierGlobal")]
[TypeId("8bd229665720411488d149d7069ea855")]
public sealed class HealModifierGlobal : HealModifier, IGlobalRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateHeal>.OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateHeal>.OnEventDidTrigger(RuleCalculateHeal evt)
	{
	}
}
