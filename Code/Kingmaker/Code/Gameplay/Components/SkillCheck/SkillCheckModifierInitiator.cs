using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[ComponentName("SkillCheck/SkillCheckModifierInitiator")]
[TypeId("d5d8bab52e3341f681725608797760d8")]
public sealed class SkillCheckModifierInitiator : SkillCheckModifier, IInitiatorRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventAboutToTrigger(RuleCalculateSkillCheck evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventDidTrigger(RuleCalculateSkillCheck evt)
	{
	}
}
