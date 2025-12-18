using System;
using Kingmaker.Controllers.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Visual.Sound;

public class PsychicPhenomenaAsksController : BaseAsksController, IGlobalRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IGlobalRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
		TryTriggerAsk(evt.ResultIsPerils);
	}

	public void OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
		TryTriggerAsk(evt.ResultIsPerils);
	}

	public AskWrapper GetPhenomenaBark(BaseUnitEntity unit, EffectsState effectsState)
	{
		if (!(unit?.View == null) && unit.View.Asks != null)
		{
			switch (effectsState)
			{
			case EffectsState.None:
				break;
			case EffectsState.PsychicPhenomena:
				return unit.View.Asks.PsychicPhenomena;
			case EffectsState.PerilsOfTheWarp:
				return unit.View.Asks.PerilsOfTheWarp;
			default:
				throw new NotImplementedException();
			}
		}
		return null;
	}

	private void TryTriggerAsk(bool isPerilsOfTheWarp)
	{
		EffectsState phenomenaState = ((!isPerilsOfTheWarp) ? EffectsState.PsychicPhenomena : EffectsState.PerilsOfTheWarp);
		BaseUnitEntity randomPartyEntity = UnitAsksHelper.GetRandomPartyEntity(delegate(BaseUnitEntity x)
		{
			if (x.LifeState.IsConscious)
			{
				AskWrapper phenomenaBark = GetPhenomenaBark(x, phenomenaState);
				if (phenomenaBark != null && phenomenaBark.HasBarks)
				{
					return !phenomenaBark.IsOnCooldown;
				}
				return false;
			}
			return false;
		});
		if (randomPartyEntity != null && randomPartyEntity.View.Asks != null)
		{
			GetPhenomenaBark(randomPartyEntity, phenomenaState)?.Schedule();
		}
	}
}
