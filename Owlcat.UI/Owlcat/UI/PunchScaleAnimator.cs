using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public class PunchScaleAnimator
{
	[Min(0f)]
	public int Count = 1;

	[Min(0.1f)]
	public float Frequency = 1f;

	[Min(0f)]
	public float Damping = 1f;

	public float Amplitude = 1f;

	public static Action<Transform, PunchScaleAnimator, float> OnUpdateDelegate = OnUpdate;

	public PunchScaleAnimator(int count, float frequency, float damping, float amplitude)
	{
		Count = Mathf.Max(0, count);
		Frequency = Mathf.Max(0.1f, frequency);
		Damping = Mathf.Max(0f, damping);
		Amplitude = amplitude;
	}

	public Transition Play(MonoBehaviour host)
	{
		return Play(host, host.transform);
	}

	public Transition Play(MonoBehaviour host, Transform target)
	{
		return Tween.Play(host, target, this, 0f, 1f, (float)Count / Frequency, OnUpdateDelegate);
	}

	public static void OnUpdate(Transform transform, PunchScaleAnimator settings, float factor)
	{
		float num = factor * (float)settings.Count / settings.Frequency;
		float num2 = Mathf.Sin(num * MathF.PI * settings.Frequency) * Mathf.Exp((0f - num) * settings.Damping);
		float num3 = 1f + settings.Amplitude * num2;
		transform.localScale = new Vector3(num3, num3, 1f);
	}
}
