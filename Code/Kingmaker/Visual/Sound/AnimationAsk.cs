using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Enums.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

[Serializable]
[IKnowWhatImDoing]
public class AnimationAsk : AsksSet
{
	public MappedAnimationEventType AnimationEvent;

	public AnimationAsk()
	{
	}

	public AnimationAsk(MappedAnimationEventType animationEvent)
	{
		AnimationEvent = animationEvent;
	}

	public override bool CheckBarkChance(float chanceValue)
	{
		if (TurnController.IsInTurnBasedCombat() && AnimationEvent == MappedAnimationEventType.IdleCombat)
		{
			return PFStatefulRandom.Visuals.Sounds.value <= chanceValue * ConfigRoot.Instance.Sound.TBMIdleAudioOverride;
		}
		return PFStatefulRandom.Visuals.Sounds.value <= chanceValue;
	}
}
