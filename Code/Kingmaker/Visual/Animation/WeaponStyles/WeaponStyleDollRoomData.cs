using System;
using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleDollRoomData : IWeaponStyleAnimationClipsProvider
{
	public AnimationClipWrapper Idle;

	public List<AnimationClipWrapper> Variants;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return Idle;
		foreach (AnimationClipWrapper variant in Variants)
		{
			yield return variant;
		}
	}
}
