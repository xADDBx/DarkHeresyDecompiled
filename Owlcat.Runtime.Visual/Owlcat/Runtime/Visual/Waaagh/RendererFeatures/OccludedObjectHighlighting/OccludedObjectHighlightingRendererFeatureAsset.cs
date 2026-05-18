using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

[CreateAssetMenu(menuName = "Renderer Features/Occluded Object Highlighting Renderer Feature")]
internal sealed class OccludedObjectHighlightingRendererFeatureAsset : RendererFeatureAsset
{
	[SerializeField]
	private Downsample m_DownsampleFactor = Downsample.None;

	[SerializeField]
	[Range(0f, 50f)]
	private int m_BlurIterations = 2;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurMinSpread = 0.65f;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurSpread = 0.25f;

	[SerializeField]
	private BlurDirections m_BlurDirections;

	[SerializeField]
	private float m_ScanLineFreq0 = 1000f;

	[SerializeField]
	private float m_ScanLineFreq1 = 1500f;

	[SerializeField]
	private float m_ScanLineSpeed = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_ScanLineOpacity = 0.5f;

	public override IRendererFeature CreateRendererFeature()
	{
		return new OccludedObjectHighlightingRendererFeature(this);
	}

	public Settings GetSettings()
	{
		Settings result = default(Settings);
		result.DownsampleFactor = m_DownsampleFactor;
		result.BlurIterations = m_BlurIterations;
		result.BlurMinSpread = m_BlurMinSpread;
		result.BlurSpread = m_BlurSpread;
		result.BlurDirections = m_BlurDirections;
		result.ScanLineFreq0 = m_ScanLineFreq0;
		result.ScanLineFreq1 = m_ScanLineFreq1;
		result.ScanLineSpeed = m_ScanLineSpeed;
		result.ScanLineOpacity = m_ScanLineOpacity;
		return result;
	}
}
