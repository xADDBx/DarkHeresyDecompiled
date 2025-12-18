using System;
using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleDisabledData : IWeaponStyleAnimationClipsProvider
{
	public AnimationClipWrapper Disabled;

	public AnimationClipWrapper DisabledIn;

	public AnimationClipWrapper DisabledOut;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return Disabled;
		yield return DisabledIn;
		yield return DisabledOut;
	}
}
