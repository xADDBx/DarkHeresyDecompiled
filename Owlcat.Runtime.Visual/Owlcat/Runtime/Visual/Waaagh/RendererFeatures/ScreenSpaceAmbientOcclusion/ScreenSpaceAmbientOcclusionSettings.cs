using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceAmbientOcclusion;

[Serializable]
public class ScreenSpaceAmbientOcclusionSettings
{
	internal enum DepthSource
	{
		Depth,
		DepthNormals
	}

	internal enum NormalQuality
	{
		Low,
		Medium,
		High
	}

	internal enum AOSampleOption
	{
		High,
		Medium,
		Low
	}

	internal enum AOMethodOptions
	{
		BlueNoise,
		InterleavedGradient
	}

	internal enum BlurQualityOptions
	{
		High,
		Medium,
		Low
	}

	[SerializeField]
	internal AOMethodOptions AOMethod;

	[SerializeField]
	internal bool Downsample;

	[SerializeField]
	internal DepthSource Source = DepthSource.DepthNormals;

	[SerializeField]
	internal NormalQuality NormalSamples = NormalQuality.Medium;

	[SerializeField]
	[Min(0f)]
	internal float Intensity = 3f;

	[SerializeField]
	[Range(0f, 1f)]
	internal float DirectLightingStrength = 0.25f;

	[SerializeField]
	[Min(0f)]
	internal float Radius = 0.035f;

	[SerializeField]
	internal AOSampleOption Samples = AOSampleOption.Medium;

	[SerializeField]
	internal BlurQualityOptions BlurQuality;

	[SerializeField]
	[Min(0f)]
	internal float Falloff = 100f;
}
