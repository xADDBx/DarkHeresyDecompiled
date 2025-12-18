using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Replace Ability/ReplaceAbilityTargetBeforeCastGlobal")]
[TypeId("b749714f4f904de78caae6c17462e168")]
public sealed class ReplaceAbilityTargetBeforeCastGlobal : ReplaceAbilityTargetBeforeCast, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RulePerformAbility>.OnEventAboutToTrigger(RulePerformAbility evt)
	{
		TryReplaceTarget(evt);
	}

	void IRulebookHandler<RulePerformAbility>.OnEventDidTrigger(RulePerformAbility evt)
	{
	}
}
