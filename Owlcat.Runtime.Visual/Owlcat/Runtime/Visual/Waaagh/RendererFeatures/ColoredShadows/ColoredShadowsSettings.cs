using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows;

[Serializable]
public class ColoredShadowsSettings
{
	private const bool kColorShowAlpha = false;

	private const bool kColorHDR = true;

	public bool Enable = true;

	[ColorUsage(false, true)]
	public Color Color = new Color(2f, 0.6f, 0f, 1f);

	[Range(0f, 1f)]
	public float ShadowThreshold = 0.15f;

	[Range(0.01f, 1f)]
	public float ShadowSmoothness = 0.9f;

	[Range(0f, 1f)]
	public float DistanceThreshold = 0.01f;

	[Range(0.01f, 1f)]
	public float DistanceSmoothness = 0.7f;

	[Range(-1f, 1f)]
	public float DiffuseThreshold = 0.15f;

	[Range(0.01f, 1f)]
	public float DiffuseSmoothness = 0.75f;
}
