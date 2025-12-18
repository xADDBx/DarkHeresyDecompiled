using System;
using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleProneData : IWeaponStyleAnimationClipsProvider
{
	public AnimationClipWrapper Prone;

	public AnimationClipWrapper ProneIn;

	public AnimationClipWrapper ProneOutCombat;

	public AnimationClipWrapper ProneOutNonCombat;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return Prone;
		yield return ProneIn;
		yield return ProneOutCombat;
		yield return ProneOutNonCombat;
	}
}
