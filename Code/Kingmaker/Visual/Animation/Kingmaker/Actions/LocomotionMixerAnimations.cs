using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class LocomotionMixerAnimations : IEnumerable<(AnimationClipWrapper ClipWrapper, Vector2 Threshold)>, IEnumerable
{
	private readonly List<(AnimationClipWrapper, Vector2)> Animations = new List<(AnimationClipWrapper, Vector2)>();

	public AnimationClipWrapper ClipWithFootstepEvents;

	public IEnumerator<(AnimationClipWrapper, Vector2)> GetEnumerator()
	{
		return Animations.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(AnimationClipWrapper clip, float normalizedSpeed, Vector2 direction)
	{
		Animations.Add((clip, direction.normalized * normalizedSpeed));
	}
}
