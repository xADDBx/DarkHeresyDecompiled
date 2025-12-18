using System;
using UnityEngine;

[Serializable]
public class AnimationSettings
{
	public float Value;

	public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float Time = 1f;
}
