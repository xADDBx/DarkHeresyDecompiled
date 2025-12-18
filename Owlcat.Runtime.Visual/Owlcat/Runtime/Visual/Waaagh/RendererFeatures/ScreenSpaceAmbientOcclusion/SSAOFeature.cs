using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceAmbientOcclusion.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceAmbientOcclusion;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/SSAO", fileName = "SSAOFeature")]
public class SSAOFeature : ScriptableRendererFeature
{
	internal const string k_AOInterleavedGradientKeyword = "_INTERLEAVED_GRADIENT";

	internal const string k_AOBlueNoiseKeyword = "_BLUE_NOISE";

	internal const string k_OrthographicCameraKeyword = "_ORTHOGRAPHIC";

	internal const string k_SourceDepthLowKeyword = "_SOURCE_DEPTH_LOW";

	internal const string k_SourceDepthMediumKeyword = "_SOURCE_DEPTH_MEDIUM";

	internal const string k_SourceDepthHighKeyword = "_SOURCE_DEPTH_HIGH";

	internal const string k_SourceDepthNormalsKeyword = "_SOURCE_DEPTH_NORMALS";

	internal const string k_SampleCountLowKeyword = "_SAMPLE_COUNT_LOW";

	internal const string k_SampleCountMediumKeyword = "_SAMPLE_COUNT_MEDIUM";

	internal const string k_SampleCountHighKeyword = "_SAMPLE_COUNT_HIGH";

	[SerializeField]
	private ScreenSpaceAmbientOcclusionSettings m_Settings = new ScreenSpaceAmbientOcclusionSettings();

	private SSAOFeatureResources m_SsaoResources;

	private RenderRuntimeTextures m_SharedTextureResources;

	private SSAOPass m_SSAOPass;

	private Material m_SSAOPassMat;

	public override void Create()
	{
		m_SsaoResources = GraphicsSettings.GetRenderPipelineSettings<SSAOFeatureResources>();
		m_SharedTextureResources = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
		if (m_SsaoResources != null && m_SsaoResources.SsaoPS != null)
		{
			m_SSAOPassMat = CoreUtils.CreateEngineMaterial(m_SsaoResources.SsaoPS);
		}
		if (m_SSAOPass == null)
		{
			m_SSAOPass = new SSAOPass(RenderPassEvent.BeforeRenderingGbuffer);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		Texture2D[] blueNoiseTextures = m_SharedTextureResources.BlueNoise256Textures;
		if (m_SSAOPass.Setup(ref m_Settings, ref m_SSAOPassMat, ref blueNoiseTextures))
		{
			renderer.EnqueuePass(m_SSAOPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		CoreUtils.Destroy(m_SSAOPassMat);
	}
}
