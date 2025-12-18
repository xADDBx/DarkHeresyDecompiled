using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Debug;
using Owlcat.Runtime.Visual.Waaagh.Passes.Debug.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugHandler
{
	internal class DebugMipMapTexture
	{
		private readonly Color[] m_DebugColors = new Color[6]
		{
			new Color(0f, 0f, 1f, 0.8f),
			new Color(0f, 0.5f, 1f, 0.4f),
			new Color(1f, 1f, 1f, 0f),
			new Color(1f, 0.7f, 0f, 0.2f),
			new Color(1f, 0.3f, 0f, 0.6f),
			new Color(1f, 0f, 0f, 0.8f)
		};

		private Texture2D m_MipMapTexture;

		public Texture2D MipMapTexture
		{
			get
			{
				if (m_MipMapTexture == null)
				{
					CreateTexture();
				}
				return m_MipMapTexture;
			}
		}

		public DebugMipMapTexture()
		{
			CreateTexture();
		}

		private void CreateTexture()
		{
			int num = 32;
			int num2 = 0;
			m_MipMapTexture = new Texture2D(num, num, TextureFormat.RGBA32, mipChain: true);
			m_MipMapTexture.name = "_MipMapDebugMap";
			while (num >= 1)
			{
				Color[] array = new Color[num * num];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = m_DebugColors[num2];
				}
				m_MipMapTexture.SetPixels(array, num2);
				num2++;
				num /= 2;
			}
			m_MipMapTexture.filterMode = FilterMode.Trilinear;
			m_MipMapTexture.Apply(updateMipmaps: false);
		}

		public void Dispose()
		{
			if (m_MipMapTexture != null)
			{
				Object.DestroyImmediate(m_MipMapTexture);
				m_MipMapTexture = null;
			}
		}
	}

	public const int kFullScreenDebugBufferBinding = 1;

	private ScriptableRendererData m_Data;

	private WaaaghDebugData m_DebugData;

	private ScriptableRenderer m_Renderer;

	private DebugMipMapTexture m_MipMapTexture;

	private DebugTerrainOverlay m_DebugTerrainOverlay;

	private bool m_IsCompletelyOverridesRendering;

	private Material m_FullscreenDebugMaterial;

	private Material m_ShadowsDebugMaterial;

	private Material m_ShowLightSortingCurveMaterial;

	private Material m_VirtualTextureDebugMaterial;

	private Material m_GPUDrivenDebugMaterial;

	private SetupDebugBuffersPass m_SetupDebugBuffersPass;

	private ApplyDebugSettingsPass m_ApplyDebugSettingsPass;

	private DrawObjectsWireframePass m_DrawObjectsWireframePass;

	private DrawObjectsOverdrawPass m_DrawObjectsOverdrawPass;

	private DepthPrePass m_DepthPrePass;

	private DebugQuadOverdrawPass m_DebugQuadOverdrawPass;

	private FullscreenDebugPass m_FullscreenDebugPass;

	private ShowLightSortingCurvePass m_ShowLightSortingCurvePass;

	private ShadowsDebugPass m_ShadowsDebugPass;

	private RenderGraphDebugResources m_Resources;

	private VirtualTextureFeedbackDebugPass m_VirtualTextureFeedbackPass;

	private VirtualTextureShowPhysicalAtlasPass m_VirtualTextureShowPhysicalAtlasPass;

	private VirtualTextureShowBatchedCopyRtPass m_VirtualTextureShowBatchedCopyRtPass;

	private VirtualTextureShowIndirectTexturePass m_VirtualTextureShowIndirectTexturePass;

	private VirtualTextureShowVirtualAtlasPass m_VirtualTextureShowVirtualAtlasPass;

	private GPUDrivenDebugPreparePass m_GPUDrivenDebugPreparePass;

	private GPUDrivenDebugFinishPass m_GPUDrivenDebugFinishPass;

	private GPUDrivenDebugShowOcclusionTestPass m_GPUDrivenDebugShowOcclusionTestPass;

	private DebugMapOverlayPass m_DebugMapOverlayPass;

	public bool IsCompletelyOverridesRendering => m_IsCompletelyOverridesRendering;

	public RenderGraphDebugResources Resources => m_Resources;

	public DebugHandler(ScriptableRendererData data, ScriptableRenderer renderer)
	{
		m_Data = data;
		m_DebugData = WaaaghPipeline.Asset.DebugData;
		m_Renderer = renderer;
		m_MipMapTexture = new DebugMipMapTexture();
		m_DebugTerrainOverlay = new DebugTerrainOverlay(m_DebugData);
		m_Resources = new RenderGraphDebugResources();
		m_FullscreenDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Resources.DebugFullscreenPS);
		m_ShadowsDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Resources.ShadowsDebugPS);
		m_ShowLightSortingCurveMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Resources.ShowLightSortingCurvePS);
		m_VirtualTextureDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Resources.VirtualTextureDebugPS);
		m_GPUDrivenDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Resources.GPUDrivenDebugPS);
		m_SetupDebugBuffersPass = new SetupDebugBuffersPass(m_Resources, m_DebugData);
		m_ApplyDebugSettingsPass = new ApplyDebugSettingsPass(RenderPassEvent.BeforeRendering, m_MipMapTexture);
		m_DrawObjectsWireframePass = new DrawObjectsWireframePass(RenderPassEvent.BeforeRenderingTransparents);
		m_DrawObjectsOverdrawPass = new DrawObjectsOverdrawPass(RenderPassEvent.AfterRenderingTransparents);
		m_DepthPrePass = new DepthPrePass(RenderPassEvent.AfterRenderingPrePasses, GBufferType.Opaque);
		m_DebugQuadOverdrawPass = new DebugQuadOverdrawPass(m_DebugData, m_Resources);
		m_FullscreenDebugPass = new FullscreenDebugPass((RenderPassEvent)1001, m_DebugData, m_Resources, m_FullscreenDebugMaterial);
		m_ShadowsDebugPass = new ShadowsDebugPass((RenderPassEvent)1001, m_DebugData, m_ShadowsDebugMaterial);
		m_VirtualTextureFeedbackPass = new VirtualTextureFeedbackDebugPass((RenderPassEvent)1001, m_DebugData, m_VirtualTextureDebugMaterial);
		m_VirtualTextureShowPhysicalAtlasPass = new VirtualTextureShowPhysicalAtlasPass((RenderPassEvent)1001, m_DebugData, m_VirtualTextureDebugMaterial);
		m_VirtualTextureShowBatchedCopyRtPass = new VirtualTextureShowBatchedCopyRtPass((RenderPassEvent)1001, m_DebugData, m_VirtualTextureDebugMaterial);
		m_VirtualTextureShowIndirectTexturePass = new VirtualTextureShowIndirectTexturePass((RenderPassEvent)1001, m_DebugData, m_VirtualTextureDebugMaterial);
		m_VirtualTextureShowVirtualAtlasPass = new VirtualTextureShowVirtualAtlasPass((RenderPassEvent)1001, m_DebugData, m_VirtualTextureDebugMaterial);
		m_GPUDrivenDebugPreparePass = new GPUDrivenDebugPreparePass(RenderPassEvent.BeforeRendering, m_DebugData);
		m_GPUDrivenDebugFinishPass = new GPUDrivenDebugFinishPass(RenderPassEvent.AfterRendering);
		m_GPUDrivenDebugShowOcclusionTestPass = new GPUDrivenDebugShowOcclusionTestPass((RenderPassEvent)1001, m_GPUDrivenDebugMaterial, m_DebugData);
		WaaaghRenderer renderer2 = m_Renderer as WaaaghRenderer;
		m_ShowLightSortingCurvePass = new ShowLightSortingCurvePass((RenderPassEvent)1001, renderer2, m_DebugData, m_ShowLightSortingCurveMaterial);
		m_DebugMapOverlayPass = new DebugMapOverlayPass((RenderPassEvent)1001, m_DebugData);
	}

	internal void Setup(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		if (waaaghCameraData.cameraType != CameraType.Game && waaaghCameraData.cameraType != CameraType.SceneView)
		{
			return;
		}
		bool wireframe = GL.wireframe;
		bool flag = false;
		bool num = m_DebugData.RenderingDebug.OverdrawMode == DebugOverdrawMode.QuadOverdraw;
		if (m_DebugData != null)
		{
			flag = m_DebugData.RenderingDebug.OverdrawMode != DebugOverdrawMode.None;
		}
		m_IsCompletelyOverridesRendering = wireframe || flag;
		if (m_IsCompletelyOverridesRendering && m_Renderer is WaaaghRenderer waaaghRenderer)
		{
			waaaghRenderer.EnqueueGPUDrivenPassesOnBeforeRendering(waaaghRenderingData, shadows: false);
			waaaghRenderer.EnqueueClearGBufferPass();
			waaaghRenderer.EnqueueGPUDrivenPassesOnMainRendering(waaaghRenderingData);
		}
		if (num)
		{
			m_Renderer.EnqueuePass(m_SetupDebugBuffersPass);
		}
		m_Renderer.EnqueuePass(m_ApplyDebugSettingsPass);
		if (wireframe)
		{
			m_Renderer.EnqueuePass(m_DrawObjectsWireframePass);
			return;
		}
		if (flag)
		{
			switch (m_DebugData.RenderingDebug.OverdrawMode)
			{
			case DebugOverdrawMode.All:
			case DebugOverdrawMode.TransparentOnly:
			case DebugOverdrawMode.OpaqueOnly:
				m_DrawObjectsOverdrawPass.OverdrawMode = m_DebugData.RenderingDebug.OverdrawMode;
				m_Renderer.EnqueuePass(m_DrawObjectsOverdrawPass);
				break;
			case DebugOverdrawMode.QuadOverdraw:
				m_Renderer.EnqueuePass(m_DepthPrePass);
				m_Renderer.EnqueuePass(m_DebugQuadOverdrawPass);
				m_Renderer.EnqueuePass(m_FullscreenDebugPass);
				break;
			}
			return;
		}
		m_Renderer.EnqueuePass(m_FullscreenDebugPass);
		if (waaaghCameraData.CameraResolveRequired)
		{
			m_Renderer.EnqueuePass(m_ShadowsDebugPass);
		}
		if (!waaaghRenderingData.VirtualTextureManager.IsVirtualAtlasEmpty)
		{
			if (m_DebugData.VirtualTextureDebug.ShowFeedback)
			{
				m_Renderer.EnqueuePass(m_VirtualTextureFeedbackPass);
			}
			if (m_DebugData.VirtualTextureDebug.ShowPhysicalAtlas)
			{
				m_Renderer.EnqueuePass(m_VirtualTextureShowPhysicalAtlasPass);
			}
			if (m_DebugData.VirtualTextureDebug.ShowIndirectionTexture)
			{
				m_Renderer.EnqueuePass(m_VirtualTextureShowIndirectTexturePass);
			}
			if (m_DebugData.VirtualTextureDebug.ShowVirtualAtlas)
			{
				m_Renderer.EnqueuePass(m_VirtualTextureShowVirtualAtlasPass);
			}
		}
		if (m_DebugData.VirtualTextureDebug.ShowBatchedCopyRt)
		{
			m_Renderer.EnqueuePass(m_VirtualTextureShowBatchedCopyRtPass);
		}
		if (waaaghRenderingData.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			m_Renderer.EnqueuePass(m_GPUDrivenDebugPreparePass);
			if (waaaghRenderingData.GPUDrivenBatchRendererGroup.Settings.OcclusionCulling && m_DebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
			{
				m_Renderer.EnqueuePass(m_GPUDrivenDebugShowOcclusionTestPass);
			}
			m_Renderer.EnqueuePass(m_GPUDrivenDebugFinishPass);
		}
		if (m_DebugData.LightingDebug.ShowLightSortingCurve)
		{
			m_Renderer.EnqueuePass(m_ShowLightSortingCurvePass);
		}
		if (m_DebugData.RenderingDebug.DebugMapOverlay != 0)
		{
			m_Renderer.EnqueuePass(m_DebugMapOverlayPass);
		}
		m_DebugTerrainOverlay.Setup();
	}

	internal void Dispose()
	{
		CoreUtils.Destroy(m_FullscreenDebugMaterial);
		CoreUtils.Destroy(m_ShadowsDebugMaterial);
		CoreUtils.Destroy(m_ShowLightSortingCurveMaterial);
		CoreUtils.Destroy(m_VirtualTextureDebugMaterial);
		CoreUtils.Destroy(m_GPUDrivenDebugMaterial);
		m_MipMapTexture.Dispose();
		m_DebugTerrainOverlay.Cleanup();
	}
}
