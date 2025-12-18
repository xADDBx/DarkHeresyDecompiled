using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoraleChangeModifierTarget")]
[TypeId("5a93684e0ae24cc88781a3f2dc3a9bf9")]
public sealed class MoraleChangeModifierTarget : MoraleChangeModifier, ITargetRulebookHandler<RuleCalculateMoraleChange>, IRulebookHandler<RuleCalculateMoraleChange>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventAboutToTrigger(RuleCalculateMoraleChange evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventDidTrigger(RuleCalculateMoraleChange evt)
	{
	}
}
