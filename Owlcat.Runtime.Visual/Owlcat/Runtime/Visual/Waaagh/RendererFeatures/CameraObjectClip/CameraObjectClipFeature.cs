using System;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Camera Object Clip")]
public class CameraObjectClipFeature : ScriptableRendererFeature
{
	[Serializable]
	public sealed class DepthClipSettings
	{
		public CameraType CameraTypes = CameraType.Game;

		[Range(0f, 1f)]
		public float ClipTreshold = 0.1f;

		public float NoiseTiling = 1f;

		public float AlphaScale = 1f;

		public float NearCameraClipDistance = 3f;

		public bool OcclusionGeometryClipEnabled;

		public Settings OcclusionGeometryClipSettings = Owlcat.Runtime.Visual.OcclusionGeometryClip.Settings.Default;
	}

	private CameraObjectClipFeatureResources m_Resources;

	[SerializeField]
	private DepthClipSettings m_DepthClipSettings = new DepthClipSettings();

	private CameraObjectClipNoiseBakePass m_NoiseBakePass;

	private CameraObjectClipSetupPass m_SetupPass;

	private Material m_NoiseBakeMaterial;

	private float m_PrevNoiseTiling;

	private RTHandle m_Noise3D;

	public DepthClipSettings Settings => m_DepthClipSettings;

	public RTHandle Noise3D => m_Noise3D;

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		frameData.Get<WaaaghCameraData>();
		if (m_PrevNoiseTiling != m_DepthClipSettings.NoiseTiling)
		{
			renderer.EnqueuePass(m_NoiseBakePass);
			m_PrevNoiseTiling = m_DepthClipSettings.NoiseTiling;
		}
		renderer.EnqueuePass(m_SetupPass);
	}

	public override void Create()
	{
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<CameraObjectClipFeatureResources>();
		m_NoiseBakeMaterial = CoreUtils.CreateEngineMaterial(m_Resources.NoiseBakeShader);
		m_Noise3D = RTHandles.Alloc(32, 32, 32, DepthBits.None, GraphicsFormat.R8_UNorm, FilterMode.Trilinear, TextureWrapMode.Repeat, TextureDimension.Tex3D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, "CameraObjectClipNoise3D");
		m_PrevNoiseTiling = -1f;
		m_NoiseBakePass = new CameraObjectClipNoiseBakePass(RenderPassEvent.BeforeRendering, this, m_NoiseBakeMaterial);
		m_SetupPass = new CameraObjectClipSetupPass(RenderPassEvent.BeforeRendering, this);
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetEnabled(Settings.OcclusionGeometryClipEnabled);
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetSettings(Settings.OcclusionGeometryClipSettings);
	}

	public void DisableFeature()
	{
		Shader.SetGlobalFloat(ShaderPropertyId._OccludedObjectHighlightingFeatureEnabled, 0f);
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_NoiseBakeMaterial);
		if (m_Noise3D != null)
		{
			RTHandles.Release(m_Noise3D);
		}
	}
}
