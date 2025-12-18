using System;
using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleCoverData : IWeaponStyleAnimationClipsProvider
{
	public AnimationClipWrapper Cover;

	public AnimationClipWrapper CoverIn;

	public AnimationClipWrapper CoverOut;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return Cover;
		yield return CoverIn;
		yield return CoverOut;
	}
}
