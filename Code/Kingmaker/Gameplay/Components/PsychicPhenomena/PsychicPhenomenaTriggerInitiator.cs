using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Psychic/PsychicPhenomenaTriggerInitiator")]
[TypeId("31dac49a4b224ae59a49c71ec074441d")]
public class PsychicPhenomenaTriggerInitiator : PsychicPhenomenaTrigger, IInitiatorRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
	}

	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
		TryTrigger(evt);
	}
}
