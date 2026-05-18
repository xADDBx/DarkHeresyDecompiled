using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("Roll/RollDifficultyModifierTarget")]
[TypeId("4b46f13d6fa641f7941b049182af8133")]
public sealed class RollDifficultyModifierTarget : RollDifficultyModifier, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>
{
	protected override StatModifierScope Scope => StatModifierScope.Owner;

	void IRulebookHandler<RuleCalculateHitChances>.OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateHitChances>.OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventAboutToTrigger(RuleCalculateSkillCheck evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventDidTrigger(RuleCalculateSkillCheck evt)
	{
	}
}
