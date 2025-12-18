using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Enums.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

[Serializable]
[Obsolete]
[IKnowWhatImDoing]
public class AnimationBark : Bark
{
	public MappedAnimationEventType AnimationEvent;

	public override bool CheckBarkChance(float chanceValue)
	{
		if (TurnController.IsInTurnBasedCombat() && AnimationEvent == MappedAnimationEventType.IdleCombat)
		{
			return PFStatefulRandom.Visuals.Sounds.value <= chanceValue * ConfigRoot.Instance.Sound.TBMIdleAudioOverride;
		}
		return PFStatefulRandom.Visuals.Sounds.value <= chanceValue;
	}
}
