using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[CreateAssetMenu(menuName = "Renderer Features/Highlighting Renderer Feature")]
internal sealed class HighlightingRendererFeatureAsset : RendererFeatureAsset
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
	private ZTestMode m_ZTestMode;

	public override IRendererFeature CreateRendererFeature()
	{
		return new HighlightingRendererFeature(this);
	}

	public Settings GetSettings()
	{
		Settings result = default(Settings);
		result.DownsampleFactor = m_DownsampleFactor;
		result.BlurIterations = m_BlurIterations;
		result.BlurMinSpread = m_BlurMinSpread;
		result.BlurSpread = m_BlurSpread;
		result.BlurDirections = m_BlurDirections;
		result.ZTestMode = m_ZTestMode;
		return result;
	}
}
