using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("Psychic/PsychicPhenomenaTriggerGlobal")]
[TypeId("fecec9052cf44047bd4ee514ac36c22c")]
public class PsychicPhenomenaTriggerGlobal : PsychicPhenomenaTrigger, IGlobalRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IGlobalRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
	}

	public void OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
		TryTrigger(evt);
	}
}
