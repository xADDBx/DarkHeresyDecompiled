using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Visual.Sound;

public class SkillCheckAsksController : BaseAsksController, IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RulePerformSkillCheck>.OnEventDidTrigger(RulePerformSkillCheck evt)
	{
		if (evt.Initiator is UnitEntity unitEntity && unitEntity.Faction.IsPlayer && !evt.Silent)
		{
			if ((evt.Voice & RulePerformSkillCheck.VoicingType.Success) != 0 && evt.ResultIsSuccess)
			{
				unitEntity.View.Asks?.CheckSuccess.Schedule();
			}
			else if ((evt.Voice & RulePerformSkillCheck.VoicingType.Failure) != 0 && !evt.ResultIsSuccess)
			{
				unitEntity.View.Asks?.CheckFail.Schedule();
			}
		}
	}

	void IRulebookHandler<RulePerformSkillCheck>.OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}
}
