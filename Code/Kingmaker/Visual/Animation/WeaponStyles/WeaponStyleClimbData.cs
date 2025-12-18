using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleClimbData : IWeaponStyleAnimationClipsProvider
{
	private const string suffix = " Right";

	[Header("Ledge")]
	public AnimationClipWrapper LedgeLowUp;

	public AnimationClipWrapper LedgeLowDown;

	public AnimationClipWrapper LedgeHighUp;

	public AnimationClipWrapper LedgeHighDown;

	[Header("Ladder")]
	[SerializeField]
	public bool IsLargeUnit;

	[Space(15f)]
	public AnimationClipWrapper LadderUp;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderUpHorizontalIn;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderUpHorizontalInLeft;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderUpHorizontalOut;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderUpHorizontalOutLeft;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderUpVerticalOut;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderUpVerticalOutLeft;

	[Space(20f)]
	public AnimationClipWrapper LadderDown;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderDownHorizontalIn;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderDownHorizontalInLeft;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderDownHorizontalOut;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderDownHorizontalOutLeft;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderDownVerticalIn;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderDownVerticalInLeft;

	[Space]
	[ConditionalSuffix(" Right", "IsLargeUnit")]
	public AnimationClipWrapper LadderDownVerticalOut;

	[ShowIf("IsLargeUnit")]
	public AnimationClipWrapper LadderDownVerticalOutLeft;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return LedgeLowUp;
		yield return LedgeLowDown;
		yield return LedgeHighUp;
		yield return LedgeHighDown;
		yield return LadderUpHorizontalIn;
		yield return LadderUpHorizontalInLeft;
		yield return LadderUp;
		yield return LadderUpVerticalOut;
		yield return LadderUpVerticalOutLeft;
		yield return LadderUpHorizontalOut;
		yield return LadderUpHorizontalOutLeft;
		yield return LadderDownHorizontalIn;
		yield return LadderDownHorizontalInLeft;
		yield return LadderDownVerticalIn;
		yield return LadderDownVerticalInLeft;
		yield return LadderDown;
		yield return LadderDownVerticalOut;
		yield return LadderDownVerticalOutLeft;
		yield return LadderDownHorizontalOut;
		yield return LadderDownHorizontalOutLeft;
	}
}
