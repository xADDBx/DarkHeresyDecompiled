using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStreamingFeedback;

[CreateAssetMenu(menuName = "Renderer Features/Terrain Streaming Feedback Renderer Feature")]
public sealed class TerrainStreamingFeedbackRendererFeatureAsset : RendererFeatureAsset
{
	[SerializeField]
	private float m_ForwardOffset = 10f;

	public float ForwardOffset => m_ForwardOffset;

	public override IRendererFeature CreateRendererFeature()
	{
		return new TerrainStreamingFeedbackRendererFeature(this);
	}
}
