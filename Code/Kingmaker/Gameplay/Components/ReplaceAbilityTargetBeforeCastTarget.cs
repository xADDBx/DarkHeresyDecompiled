using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Replace Ability/ReplaceAbilityTargetBeforeCastTarget")]
[TypeId("9900d68024b642af85b921d8f0f9c22d")]
public sealed class ReplaceAbilityTargetBeforeCastTarget : ReplaceAbilityTargetBeforeCast, ITargetRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RulePerformAbility>.OnEventAboutToTrigger(RulePerformAbility evt)
	{
		TryReplaceTarget(evt);
	}

	void IRulebookHandler<RulePerformAbility>.OnEventDidTrigger(RulePerformAbility evt)
	{
	}
}
