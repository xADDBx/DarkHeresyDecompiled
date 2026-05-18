using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

[CreateAssetMenu(menuName = "Renderer Features/Terrain Blending Renderer Feature")]
internal sealed class TerrainBlendingRendererFeatureAsset : RendererFeatureAsset
{
	public enum DebugModeType
	{
		Disabled,
		IntensityBlend,
		SurfaceNormalBlend,
		SlopeFactor
	}

	[SerializeField]
	private RenderingLayerMask m_RenderingLayerMask;

	[SerializeField]
	[Min(0f)]
	private float m_BlendingOffset = 0.05f;

	[SerializeField]
	[Min(0.01f)]
	private float m_BlendingDepth = 0.5f;

	[SerializeField]
	[Min(0f)]
	private float m_BlendingNoiseTiling = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_BlendingNoiseStrength = 1f;

	[SerializeField]
	[Range(0.001f, 1f)]
	private float m_SurfaceNormalBlendFactor = 1f;

	[SerializeField]
	[MinMaxSlider(0f, 1f)]
	private Vector2 m_SurfaceNormalBlendSlopeRange = new Vector2(0.25f, 0.75f);

	[SerializeField]
	private DebugModeType m_DebugMode;

	public RenderingLayerMask RenderingLayerMask => m_RenderingLayerMask;

	public float BlendingOffset => m_BlendingOffset;

	public float BlendingDepth => m_BlendingDepth;

	public float BlendingNoiseTiling => m_BlendingNoiseTiling;

	public float BlendingNoiseStrength => m_BlendingNoiseStrength;

	public float SurfaceNormalBlendFactor => m_SurfaceNormalBlendFactor;

	public Vector2 SurfaceNormalBlendSlopeRange => m_SurfaceNormalBlendSlopeRange;

	public DebugModeType DebugMode => m_DebugMode;

	public override IRendererFeature CreateRendererFeature()
	{
		return new TerrainBlendingRendererFeature(this);
	}
}
