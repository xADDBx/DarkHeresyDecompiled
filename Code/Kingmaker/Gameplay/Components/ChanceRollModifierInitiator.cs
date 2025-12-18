using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("dfdb271b54a448c6b9aa747dfc4a1040")]
[ComponentName("Roll/ChanceRollModifierInitiator")]
public sealed class ChanceRollModifierInitiator : ChanceRollModifier, IInitiatorRulebookHandler<RuleRollChance>, IRulebookHandler<RuleRollChance>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleRollChance>.OnEventAboutToTrigger(RuleRollChance evt)
	{
		TryApplyBefore(evt);
	}

	void IRulebookHandler<RuleRollChance>.OnEventDidTrigger(RuleRollChance evt)
	{
		TryApplyAfter(evt);
	}
}
