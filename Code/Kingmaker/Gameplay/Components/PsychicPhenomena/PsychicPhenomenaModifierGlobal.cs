using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[TypeId("8156dadcfd2f407db91e7cd5c65ef6db")]
[ComponentName("Psychic/PsychicPhenomenaModifierGlobal")]
public class PsychicPhenomenaModifierGlobal : PsychicPhenomenaModifier, IGlobalRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RulePerformPsychicPhenomena>.OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
	}
}
