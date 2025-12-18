using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoralePhaseDurationModifierTarget")]
[TypeId("24204321d9284f968a94de56383c2c32")]
public sealed class MoralePhaseDurationModifierTarget : MoralePhaseDurationModifier, ITargetRulebookHandler<RuleCalculateMoralePhaseDuration>, IRulebookHandler<RuleCalculateMoralePhaseDuration>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateMoralePhaseDuration>.OnEventAboutToTrigger(RuleCalculateMoralePhaseDuration evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateMoralePhaseDuration>.OnEventDidTrigger(RuleCalculateMoralePhaseDuration evt)
	{
	}
}
