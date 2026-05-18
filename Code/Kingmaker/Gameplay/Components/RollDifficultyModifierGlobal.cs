using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("49da8b8c8ddf49cbb4bb9f2e5412159e")]
[ComponentName("Roll/RollDifficultyModifierGlobal")]
public sealed class RollDifficultyModifierGlobal : RollDifficultyModifier, IGlobalRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>, IStatModifier
{
	protected override StatModifierScope Scope => StatModifierScope.Global;

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
