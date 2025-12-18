using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("49da8b8c8ddf49cbb4bb9f2e5412159e")]
[ComponentName("Roll/RollDifficultyModifierGlobal")]
public sealed class RollDifficultyModifierGlobal : RollDifficultyModifier, IGlobalRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RuleCalculateDefence>, IRulebookHandler<RuleCalculateDefence>, IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>
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
