using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.RuleDOT;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[ComponentName("DOT/DOTRankModifierTarget")]
[TypeId("8c4fbee4c8dc48b8baae6965f9f15539")]
public sealed class DOTRankModifierTarget : DOTRankModifier, ITargetRulebookHandler<RuleCalculateDOT>, IRulebookHandler<RuleCalculateDOT>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateDOT>.OnEventAboutToTrigger(RuleCalculateDOT evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDOT>.OnEventDidTrigger(RuleCalculateDOT evt)
	{
	}
}
