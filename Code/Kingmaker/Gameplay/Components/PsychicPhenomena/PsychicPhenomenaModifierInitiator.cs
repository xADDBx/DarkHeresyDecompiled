using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[ComponentName("Psychic/PsychicPhenomenaModifierInitiator")]
[TypeId("013d1e98b26a4c02859d1857c753d73e")]
public class PsychicPhenomenaModifierInitiator : PsychicPhenomenaModifier, IInitiatorRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
	}
}
