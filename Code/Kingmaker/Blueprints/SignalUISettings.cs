using System;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
public class SignalUISettings
{
	public Gradient ColorGradient;

	public Color JammerColor;

	public Vector2 WavesTimeMinMax = new Vector2(2f, 1f);

	public Vector2 WavesDelayMinMax = new Vector2(1f, 0.5f);

	public Color GetColor(float lerpFactor)
	{
		return ColorGradient.Evaluate(lerpFactor);
	}

	public float GetWavesTime(float lerpFactor)
	{
		return Mathf.Lerp(WavesTimeMinMax.x, WavesTimeMinMax.y, lerpFactor);
	}

	public float GetWavesDelay(float lerpFactor)
	{
		return Mathf.Lerp(WavesDelayMinMax.x, WavesDelayMinMax.y, lerpFactor);
	}
}
