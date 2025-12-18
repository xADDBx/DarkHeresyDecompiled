using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoraleChangeModifierInitiator")]
[TypeId("bddb701faf5c4c14916f2c7d4b264652")]
public sealed class MoraleChangeModifierInitiator : MoraleChangeModifier, IInitiatorRulebookHandler<RuleCalculateMoraleChange>, IRulebookHandler<RuleCalculateMoraleChange>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventAboutToTrigger(RuleCalculateMoraleChange evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventDidTrigger(RuleCalculateMoraleChange evt)
	{
	}
}
