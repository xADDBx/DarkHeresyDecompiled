using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Daltonization")]
public class Daltonization : VolumeComponent, IPostProcessComponent
{
	public const float kBrightnessFactorDefaultValue = 0.5f;

	public const float kContrastFactorDefaultValue = 0.5f;

	public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter ProtanopiaFactor = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter DeuteranopiaFactor = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter TritanopiaFactor = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter BrightnessFactor = new ClampedFloatParameter(0.5f, 0f, 1f);

	public ClampedFloatParameter ContrastFactor = new ClampedFloatParameter(0.5f, 0f, 1f);

	public bool IsActive()
	{
		if (Intensity.value > 0f)
		{
			if (!(ProtanopiaFactor.value > 0f) && !(DeuteranopiaFactor.value > 0f) && !(TritanopiaFactor.value > 0f) && Mathf.Approximately(0.5f, BrightnessFactor.value))
			{
				return !Mathf.Approximately(0.5f, ContrastFactor.value);
			}
			return true;
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
