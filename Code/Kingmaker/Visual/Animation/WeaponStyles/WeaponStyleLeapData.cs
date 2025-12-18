using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleLeapData : IWeaponStyleAnimationClipsProvider
{
	[Header("Leap")]
	public AnimationClipWrapper LeapOneCell;

	public AnimationClipWrapper LeapTwoCell;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return LeapOneCell;
		yield return LeapTwoCell;
	}
}
