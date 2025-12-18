using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoralePhaseDurationModifierGlobal")]
[TypeId("3ebc2e35a2734d3b83e1b75b9cc7cef6")]
public sealed class MoralePhaseDurationModifierGlobal : MoralePhaseDurationModifier, IGlobalRulebookHandler<RuleCalculateMoralePhaseDuration>, IRulebookHandler<RuleCalculateMoralePhaseDuration>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateMoralePhaseDuration>.OnEventAboutToTrigger(RuleCalculateMoralePhaseDuration evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateMoralePhaseDuration>.OnEventDidTrigger(RuleCalculateMoralePhaseDuration evt)
	{
	}
}
