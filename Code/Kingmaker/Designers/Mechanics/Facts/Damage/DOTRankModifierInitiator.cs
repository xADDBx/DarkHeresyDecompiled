using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.RuleDOT;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[ComponentName("DOT/DOTRankModifierInitiator")]
[TypeId("fd6891b48ce74b66a1d7132913a8ec30")]
public sealed class DOTRankModifierInitiator : DOTRankModifier, IInitiatorRulebookHandler<RuleCalculateDOT>, IRulebookHandler<RuleCalculateDOT>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateDOT>.OnEventAboutToTrigger(RuleCalculateDOT evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDOT>.OnEventDidTrigger(RuleCalculateDOT evt)
	{
	}
}
