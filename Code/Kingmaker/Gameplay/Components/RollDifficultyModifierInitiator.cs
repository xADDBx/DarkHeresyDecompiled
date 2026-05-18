using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("2492273f6c0b4c05a3738cbe194a1d38")]
[ComponentName("Roll/RollDifficultyModifierInitiator")]
public sealed class RollDifficultyModifierInitiator : RollDifficultyModifier, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>
{
	protected override StatModifierScope Scope => StatModifierScope.Against;

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
