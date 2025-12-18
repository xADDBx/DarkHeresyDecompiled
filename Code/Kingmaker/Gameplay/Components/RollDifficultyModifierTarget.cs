using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Roll/RollDifficultyModifierTarget")]
[TypeId("4b46f13d6fa641f7941b049182af8133")]
public sealed class RollDifficultyModifierTarget : RollDifficultyModifier, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateDefence>, IRulebookHandler<RuleCalculateDefence>, ITargetRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>
{
	void IRulebookHandler<RuleCalculateHitChances>.OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateHitChances>.OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	void IRulebookHandler<RuleCalculateDefence>.OnEventAboutToTrigger(RuleCalculateDefence evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDefence>.OnEventDidTrigger(RuleCalculateDefence evt)
	{
	}

	void IRulebookHandler<RulePerformSkillCheck>.OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RulePerformSkillCheck>.OnEventDidTrigger(RulePerformSkillCheck evt)
	{
	}
}
