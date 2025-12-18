using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Replace Ability/ReplaceAbilityTargetBeforeCastInitiator")]
[TypeId("3e34ebe6444d4e048ca4b75b9e86de81")]
public sealed class ReplaceAbilityTargetBeforeCastInitiator : ReplaceAbilityTargetBeforeCast, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RulePerformAbility>.OnEventAboutToTrigger(RulePerformAbility evt)
	{
		TryReplaceTarget(evt);
	}

	void IRulebookHandler<RulePerformAbility>.OnEventDidTrigger(RulePerformAbility evt)
	{
	}
}
