using System;
using System.Collections.Generic;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleLocomotionData : IWeaponStyleAnimationClipsProvider
{
	[Header("Idle")]
	[ValidateNotNull]
	public AnimationClipWrapper Idle;

	public List<AnimationClipWrapper> Variants;

	[Header("Walk")]
	public AnimationClipWrapper Walk;

	public AnimationClipWrapper WalkIn;

	public AnimationClipWrapper WalkOut;

	public AnimationClipWrapper WalkLeft;

	public AnimationClipWrapper WalkRight;

	public AnimationClipWrapper WalkBack;

	[Header("Run")]
	public AnimationClipWrapper Run;

	public AnimationClipWrapper RunIn;

	public AnimationClipWrapper RunOut;

	public AnimationClipWrapper RunLeft;

	public AnimationClipWrapper RunRight;

	public AnimationClipWrapper RunBack;

	[Header("Sprint")]
	public AnimationClipWrapper Sprint;

	public AnimationClipWrapper SprintIn;

	public AnimationClipWrapper SprintOut;

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return Idle;
		foreach (AnimationClipWrapper variant in Variants)
		{
			yield return variant;
		}
		yield return Walk;
		yield return WalkIn;
		yield return WalkOut;
		yield return WalkLeft;
		yield return WalkRight;
		yield return WalkBack;
		yield return Run;
		yield return RunIn;
		yield return RunOut;
		yield return RunLeft;
		yield return RunRight;
		yield return RunBack;
		yield return Sprint;
		yield return SprintIn;
		yield return SprintOut;
	}
}
