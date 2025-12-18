using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[ComponentName("Morale/MoraleChangeModifierGlobal")]
[TypeId("4a230a18f9be48ab8b17f5816bce60b7")]
public sealed class MoraleChangeModifierGlobal : MoraleChangeModifier, IGlobalRulebookHandler<RuleCalculateMoraleChange>, IRulebookHandler<RuleCalculateMoraleChange>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventAboutToTrigger(RuleCalculateMoraleChange evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateMoraleChange>.OnEventDidTrigger(RuleCalculateMoraleChange evt)
	{
	}
}
