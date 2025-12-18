using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleDefenceData : IWeaponStyleAnimationClipsProvider
{
	public List<AnimationClipWrapper> DefenceClips;

	public List<AnimationClipWrapper> DefenceInCoverClips;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		return DefenceClips.Concat(DefenceInCoverClips);
	}
}
