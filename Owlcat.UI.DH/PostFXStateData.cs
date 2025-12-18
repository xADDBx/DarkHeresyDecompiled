using System;
using UnityEngine;

[Serializable]
public class PostFXStateData
{
	public bool Loop;

	[Header("Chromatic Intensity")]
	public AnimationSettings ChromaticIntensity;

	[Header("Lens")]
	public AnimationSettings LensDistortionIntensity;

	public AnimationSettings LensDistortionXMultiplier;

	public AnimationSettings LensDistortionYMultiplier;

	[Header("Bloom")]
	public AnimationSettings BloomIntensity;

	[Header("Film Grain")]
	public AnimationSettings FilmGrainIntensity;

	[Header("Scale")]
	public AnimationSettings ScaleX;

	public AnimationSettings ScaleY;

	public AnimationSettings ScaleZ;
}
