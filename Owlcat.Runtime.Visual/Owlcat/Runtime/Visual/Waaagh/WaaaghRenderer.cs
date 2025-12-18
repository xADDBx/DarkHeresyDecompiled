using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Base;
using Owlcat.Runtime.Visual.Waaagh.Passes.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.Passes.ProbeVolumes;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghRenderer : ScriptableRenderer
{
	private enum NoiseBasedPostProcessingStage
	{
		None,
		PostProcessPass,
		FinalPostProcessPass,
		UpscaleBlitPass
	}

	private WaaaghRendererData m_Settings;

	private readonly Material m_CopyDepthMaterial;

	private readonly Material m_ErrorMaterial;

	private readonly Material m_DeferredLightingMaterial;

	private readonly Material m_BlitMaterial;

	private readonly Material m_CoreBlitMaterial;

	private readonly Material m_BlitHDRMaterial;

	private readonly Material m_ColorPyramidMaterial;

	private readonly Material m_ApplyDistortionMaterial;

	private readonly Material m_DebugFullscreenMaterial;

	private readonly Material m_DBufferBlitMaterial;

	private readonly Material m_FogMaterial;

	private readonly Material m_SsrResolveMaterial;

	private readonly Material m_CameraMotionVectorsMaterial;

	private readonly Material m_ObjectMotionVectorsMaterial;

	private readonly Material m_CopyShadowsMaterial;

	private RenderRuntimeTextures m_TextureResources;

	private WaaaghLights m_WaaaghLights;

	private readonly WaaaghReflectionProbes m_WaaaghReflectionProbes;

	private readonly DeferredReflectionProbeBatcher m_ReflectionProbeBatcher;

	private InitializeRenderStatePass m_InitializeRenderStatePass;

	private SetupProbeVolumesPass m_SetupProbeVolumesPass;

	private DitheringSetupPass m_DitheringSetupPass;

	private DitheringSetupPass m_JitteredDitheringSetupPass;

	private SetupTranslucencyProfilesPass m_SetupTranslucencyProfilesPass;

	private CullingPass m_IRSCullingPass;

	private SubmitPass m_IRSSubmitPass;

	private ClearPass m_ClearPass;

	private ClearGBufferPass m_ClearGBufferPass;

	private NativeShadowCasterPass m_NativeShadowCasterPass;

	private GBufferPass m_GBufferPass;

	private TerrainPass m_TerrainPass;

	private CopyDepthPass m_CopyDepthAfterGBufferPass;

	private ComputeTilesMinMaxZPass m_ComputeTilesMinMaxZPass;

	private DrawDecalsPass m_DrawDecalsPass;

	private DepthPrePass m_DepthPrePass;

	private DepthPrePass m_DepthDistortionPrePass;

	private SetupLightDataPass m_SetupLightDataPass;

	private LightCullingPass m_LightCullingPass;

	private DeferredLightingPass m_DeferredLightingPass;

	private DeferredLightingBuildVariantsPass m_DeferredLightingBuildVariantsPass;

	private DeferredLightingComputePass m_DeferredLightingComputePass;

	private SetupReflectionProbesPass m_SetupReflectionProbesPass;

	private GPUDrivenForwardReflectionProbesPass m_GPUDrivenForwardReflectionProbesPass;

	private DrawObjectsWithErrorPass m_DrawObjectsWithUnsupportedMaterials;

	private DrawObjectsWithErrorPass m_DrawObjectsWithMissingMaterials;

	private DrawSkyboxPass m_DrawSkyboxPass;

	private DrawColorPyramidPass m_DrawColorPyramidAfterOpaquePass;

	private GBufferPass m_GBufferDistortionPass;

	private CopyDepthPass m_CopyDepthAfterOpaqueDistortion;

	private DrawObjectsPass m_DrawOpaqueDistortionPass;

	private DepthPyramidPass m_DepthPyramidPass;

	private MotionVectorsPass m_MotionVectorsPass;

	private ScreenSpaceReflectionsPass m_ScreenSpaceReflectionsPass;

	private DeferredReflectionsPass m_DeferredReflectionsPass;

	private FogPass m_FogPass;

	private DrawObjectsPass m_DrawTransparentPass;

	private DrawColorPyramidPass m_DrawColorPyramidAfterTransparentPass;

	private DrawDistortionVectorsPass m_DrawDistortionVectorsPass;

	private RawHistoryPass m_RawHistoryPass;

	private readonly DrawObjectsPass m_RenderOverlayForwardPass;

	private DrawDecalsPass m_DrawGUIDecalsPass;

	private CapturePass m_CapturePass;

	private GPUDrivenCullingPreparePass m_GPUDrivenCullingPreparePass;

	private GPUDrivenCullingPass m_GPUDrivenShadowCullingPass;

	private GPUDrivenCullingPass m_GPUDrivenMainCullingPass;

	private GPUDrivenCullingPass m_GPUDrivenMainFirstCullingPass;

	private DepthPyramidPass m_GPUDrivenHDBPass;

	private GPUDrivenCullingPass m_GPUDrivenMainFalseNegativeCullingPass;

	private GPUDrivenCullingDepthReprojectionPass m_GPUDrivenCullingDepthReprojectionPass;

	private GPUDrivenCullingDepthHistoryPass m_GPUDrivenCullingDepthHistoryPass;

	private FinalBlitPass m_FinalBlitPass;

	private DrawScreenSpaceUIPass m_DrawUGUIOverlayPass;

	private DrawScreenSpaceUIPass m_DrawIMGUIOverlayPass;

	private PostProcessPasses m_PostProcessPasses;

	private CopyDepthPass m_FinalCopyDepthPass;

	private InvokeOnRenderObjectCallbackPass m_InvokeOnRenderObjectCallbackPass;

	private ComputeBuffer m_DummyComputeBuffer;

	public UISubset? OverlayUIMask;

	public WaaaghRendererData Settings => m_Settings;

	internal WaaaghLights WaaaghLights => m_WaaaghLights;

	public WaaaghRenderer(WaaaghRendererData settings)
		: base(settings)
	{
		m_Settings = settings;
		m_TextureResources = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
		m_WaaaghLights = new WaaaghLights(this);
		m_WaaaghReflectionProbes = new WaaaghReflectionProbes();
		m_ReflectionProbeBatcher = new DeferredReflectionProbeBatcher(m_WaaaghReflectionProbes, settings);
		m_DummyComputeBuffer = CreateDummyComputeBuffer();
		m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.CopyDepthSimplePS);
		m_CopyDepthMaterial.SetFloat("_DepthBlendOp", SystemInfo.usesReversedZBuffer ? 4 : 3);
		m_ErrorMaterial = CoreUtils.CreateEngineMaterial("Hidden/InternalErrorShader");
		m_DeferredLightingMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.DeferredLightingShader);
		m_BlitMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.BlitShader);
		m_CoreBlitMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.CoreBlitPS);
		m_BlitHDRMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.BlitHDROverlayPS);
		m_ColorPyramidMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.ColorPyramidShader);
		m_ApplyDistortionMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.ApplyDistortionShader);
		m_DBufferBlitMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.DBufferBlitShader);
		m_FogMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.FogShader);
		m_SsrResolveMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.ScreenSpaceReflectionsShaderPS);
		m_CameraMotionVectorsMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.CameraMotionVectorsPS);
		m_ObjectMotionVectorsMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.ObjectMotionVectorsPS);
		m_CopyShadowsMaterial = CoreUtils.CreateEngineMaterial(m_Settings.Shaders.CopyCachedShadowsPS);
		IndirectRenderingSystem.Instance.Initialize(m_Settings.Shaders.IndirectRenderingCullShader);
		if (WaaaghPipeline.Asset.DebugData != null && WaaaghPipeline.Asset.DebugData.Resources.DebugFullscreenPS != null)
		{
			m_DebugFullscreenMaterial = CoreUtils.CreateEngineMaterial(WaaaghPipeline.Asset.DebugData.Resources.DebugFullscreenPS);
		}
		m_InitializeRenderStatePass = new InitializeRenderStatePass(RenderPassEvent.BeforeRenderingInternal, m_WaaaghLights);
		m_SetupProbeVolumesPass = new SetupProbeVolumesPass(RenderPassEvent.BeforeRenderingInternal);
		m_DitheringSetupPass = new DitheringSetupPass(RenderPassEvent.BeforeRenderingInternal, allowJitter: false);
		m_JitteredDitheringSetupPass = new DitheringSetupPass(RenderPassEvent.BeforeRenderingGbuffer, allowJitter: true);
		m_SetupTranslucencyProfilesPass = new SetupTranslucencyProfilesPass(RenderPassEvent.BeforeRenderingInternal);
		m_IRSCullingPass = new CullingPass(RenderPassEvent.BeforeRendering);
		m_IRSSubmitPass = new SubmitPass(RenderPassEvent.BeforeRendering);
		m_ClearPass = new ClearPass(RenderPassEvent.BeforeRenderingPrePasses);
		m_ClearGBufferPass = new ClearGBufferPass(RenderPassEvent.BeforeRenderingPrePasses);
		m_NativeShadowCasterPass = new NativeShadowCasterPass(RenderPassEvent.BeforeRenderingShadows, m_CopyShadowsMaterial);
		m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer, GBufferType.Opaque);
		m_TerrainPass = new TerrainPass(RenderPassEvent.BeforeRenderingGbuffer);
		m_CopyDepthAfterGBufferPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingGbuffer, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Intermediate, CopyDepthPass.PassCullingCriteria.Opaque);
		m_ComputeTilesMinMaxZPass = new ComputeTilesMinMaxZPass(RenderPassEvent.BeforeRenderingGbuffer, m_Settings.Shaders.ComputeTilesMinMaxZCS, m_WaaaghLights);
		m_DrawDecalsPass = new DrawDecalsPass(RenderPassEvent.BeforeRenderingGbuffer, m_DBufferBlitMaterial, drawGUIDecals: false);
		m_DepthPrePass = new DepthPrePass(RenderPassEvent.AfterRenderingPrePasses, GBufferType.Opaque);
		m_DepthDistortionPrePass = new DepthPrePass(RenderPassEvent.AfterRenderingSkybox, GBufferType.OpaqueDistortion);
		m_SetupLightDataPass = new SetupLightDataPass(RenderPassEvent.BeforeRenderingGbuffer, m_WaaaghLights);
		m_LightCullingPass = new LightCullingPass(RenderPassEvent.BeforeRenderingGbuffer, m_Settings.Shaders.LightCullingShader, m_WaaaghLights);
		m_MotionVectorsPass = new MotionVectorsPass((RenderPassEvent)218, m_CameraMotionVectorsMaterial, m_ObjectMotionVectorsMaterial);
		m_DeferredLightingBuildVariantsPass = new DeferredLightingBuildVariantsPass(RenderPassEvent.AfterRenderingGbuffer, m_Settings.Shaders.DeferredLightingBuildFeatureTilesCS, m_Settings.Shaders.DeferredLightingBuildFeatureTilesListsCS);
		m_DeferredLightingComputePass = new DeferredLightingComputePass(RenderPassEvent.AfterRenderingGbuffer, SystemSupportsDxcCompiler() ? m_Settings.Shaders.DeferredLightingDxcCS : m_Settings.Shaders.DeferredLightingCS);
		m_DeferredLightingPass = new DeferredLightingPass(RenderPassEvent.AfterRenderingGbuffer, m_DeferredLightingMaterial);
		m_SetupReflectionProbesPass = new SetupReflectionProbesPass(RenderPassEvent.AfterRenderingSkybox, m_WaaaghReflectionProbes);
		m_GPUDrivenForwardReflectionProbesPass = new GPUDrivenForwardReflectionProbesPass(RenderPassEvent.BeforeRenderingTransparents);
		m_DrawObjectsWithUnsupportedMaterials = new DrawObjectsWithErrorPass(RenderPassEvent.BeforeRenderingOpaques, m_ErrorMaterial, DrawObjectsWithErrorPass.ErrorType.UnsupportedMaterials);
		m_DrawObjectsWithMissingMaterials = new DrawObjectsWithErrorPass(RenderPassEvent.BeforeRenderingOpaques, m_ErrorMaterial, DrawObjectsWithErrorPass.ErrorType.MissingMaterial);
		m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
		m_DrawColorPyramidAfterOpaquePass = new DrawColorPyramidPass(RenderPassEvent.BeforeRenderingSkybox, ColorPyramidType.OpaqueDistortion, m_ColorPyramidMaterial, m_BlitMaterial);
		GPUDrivenDepthReprojectionUtils depthReprojectionUtils = new GPUDrivenDepthReprojectionUtils(m_CopyDepthMaterial);
		DepthPyramidGenerationUtils depthPyramidGenerationUtils = new DepthPyramidGenerationUtils(m_Settings);
		m_GBufferDistortionPass = new GBufferPass(RenderPassEvent.AfterRenderingSkybox, GBufferType.OpaqueDistortion);
		m_DrawOpaqueDistortionPass = new DrawObjectsPass(RenderPassEvent.AfterRenderingSkybox, DrawObjectsPass.RendererListType.OpaqueDistortionForward);
		m_CopyDepthAfterOpaqueDistortion = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Intermediate, CopyDepthPass.PassCullingCriteria.OpaqueDistortion);
		m_DepthPyramidPass = new DepthPyramidPass(RenderPassEvent.AfterRenderingSkybox, depthPyramidGenerationUtils, depthReprojectionUtils, "DepthPyramidPass", WaaaghProfileId.DepthPyramidPass);
		m_ScreenSpaceReflectionsPass = new ScreenSpaceReflectionsPass(RenderPassEvent.AfterRenderingSkybox, m_Settings.Shaders.StochasticScreenSpaceReflectionsCS, m_BlitMaterial, m_SsrResolveMaterial, m_TextureResources);
		m_DeferredReflectionsPass = new DeferredReflectionsPass(RenderPassEvent.AfterRenderingSkybox, m_Settings.Shaders.DeferredReflectionsMaterial, m_ReflectionProbeBatcher, m_Settings.Shaders.BilateralUpsampleCS);
		m_FogPass = new FogPass(RenderPassEvent.AfterRenderingSkybox, m_FogMaterial);
		m_DrawTransparentPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingTransparents, DrawObjectsPass.RendererListType.Transparent);
		m_DrawColorPyramidAfterTransparentPass = new DrawColorPyramidPass(RenderPassEvent.BeforeRenderingTransparents, ColorPyramidType.TransparentDistortion, m_ColorPyramidMaterial, m_BlitMaterial);
		m_DrawDistortionVectorsPass = new DrawDistortionVectorsPass(RenderPassEvent.BeforeRenderingTransparents, m_ApplyDistortionMaterial);
		m_RawHistoryPass = new RawHistoryPass(RenderPassEvent.BeforeRenderingTransparents, m_BlitMaterial);
		m_RenderOverlayForwardPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingTransparents, DrawObjectsPass.RendererListType.Overlay);
		m_DrawGUIDecalsPass = new DrawDecalsPass(RenderPassEvent.BeforeRenderingTransparents, null, drawGUIDecals: true);
		m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
		m_GPUDrivenCullingPreparePass = new GPUDrivenCullingPreparePass(RenderPassEvent.BeforeRendering);
		m_GPUDrivenShadowCullingPass = new GPUDrivenCullingPass(RenderPassEvent.BeforeRendering, GPUDrivenCullingPass.OcclusionCullingPassType.None, (BatchCullingViewType vt, GPUDrivenRendererGroupPool.ViewType _) => vt == BatchCullingViewType.Light, GPUDrivenCullingPass.GeometryPassType.Shadows);
		m_GPUDrivenMainCullingPass = new GPUDrivenCullingPass(RenderPassEvent.BeforeRendering, GPUDrivenCullingPass.OcclusionCullingPassType.None, (BatchCullingViewType vt, GPUDrivenRendererGroupPool.ViewType _) => vt == BatchCullingViewType.Camera);
		m_GPUDrivenMainFirstCullingPass = new GPUDrivenCullingPass(RenderPassEvent.BeforeRenderingPrePasses, GPUDrivenCullingPass.OcclusionCullingPassType.First, (BatchCullingViewType _, GPUDrivenRendererGroupPool.ViewType vt) => vt == GPUDrivenRendererGroupPool.ViewType.DepthOnly);
		m_GPUDrivenCullingDepthReprojectionPass = new GPUDrivenCullingDepthReprojectionPass(RenderPassEvent.AfterRenderingPrePasses, depthReprojectionUtils);
		bool depthReprojection = WaaaghPipeline.Asset.GPUDrivenBRGSettings.DepthReprojection;
		m_GPUDrivenHDBPass = new DepthPyramidPass(RenderPassEvent.AfterRenderingPrePasses, depthPyramidGenerationUtils, depthReprojectionUtils, "GPUDrivenCulling.HDBPass", WaaaghProfileId.GPUDrivenCullingPass_HDB, useMax: true, depthReprojection);
		m_GPUDrivenMainFalseNegativeCullingPass = new GPUDrivenCullingPass(RenderPassEvent.AfterRenderingPrePasses, GPUDrivenCullingPass.OcclusionCullingPassType.FalseNegative, (BatchCullingViewType _, GPUDrivenRendererGroupPool.ViewType vt) => vt == GPUDrivenRendererGroupPool.ViewType.Camera || vt == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors);
		m_GPUDrivenCullingDepthHistoryPass = new GPUDrivenCullingDepthHistoryPass(RenderPassEvent.AfterRenderingOpaques, depthPyramidGenerationUtils);
		m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, m_CoreBlitMaterial, m_BlitHDRMaterial);
		PostProcessParams parameters = PostProcessParams.Create();
		parameters.BlitMaterial = m_BlitMaterial;
		parameters.RequestColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((bool)asset)
		{
			parameters.RequestColorFormat = WaaaghPipeline.MakeRenderTextureGraphicsFormat(asset.SupportsHDR, asset.HDRColorBufferPrecision, needsAlpha: false);
		}
		m_PostProcessPasses = new PostProcessPasses(settings.PostProcessResources, ref parameters);
		m_DrawUGUIOverlayPass = new DrawScreenSpaceUIPass(UISubset.UIToolkit_UGUI, RenderPassEvent.InternalRenderingOverlayUI);
		m_DrawIMGUIOverlayPass = new DrawScreenSpaceUIPass(UISubset.LowLevel, RenderPassEvent.AfterRendering);
		m_FinalCopyDepthPass = new CopyDepthPass((RenderPassEvent)1009, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Final, CopyDepthPass.PassCullingCriteria.None);
		m_InvokeOnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
	}

	protected override void Dispose(bool disposing)
	{
		m_WaaaghLights.Dispose();
		m_WaaaghReflectionProbes.Dispose();
		m_ReflectionProbeBatcher.Dispose();
		CoreUtils.Destroy(m_CopyDepthMaterial);
		CoreUtils.Destroy(m_ErrorMaterial);
		CoreUtils.Destroy(m_DeferredLightingMaterial);
		CoreUtils.Destroy(m_BlitMaterial);
		CoreUtils.Destroy(m_CoreBlitMaterial);
		CoreUtils.Destroy(m_BlitHDRMaterial);
		CoreUtils.Destroy(m_ColorPyramidMaterial);
		CoreUtils.Destroy(m_ApplyDistortionMaterial);
		CoreUtils.Destroy(m_DebugFullscreenMaterial);
		CoreUtils.Destroy(m_DBufferBlitMaterial);
		CoreUtils.Destroy(m_FogMaterial);
		CoreUtils.Destroy(m_SsrResolveMaterial);
		CoreUtils.Destroy(m_CameraMotionVectorsMaterial);
		CoreUtils.Destroy(m_ObjectMotionVectorsMaterial);
		CoreUtils.Destroy(m_CopyShadowsMaterial);
		m_DummyComputeBuffer.Release();
		m_PostProcessPasses.Dispose();
		IndirectRenderingSystem.Instance.Dispose();
	}

	protected override void Setup(ScriptableRenderContext context, ContextContainer frameData)
	{
		OnBeforeRendering(context, frameData);
		DebugHandler debugHandler = base.DebugHandler;
		if (debugHandler == null || !debugHandler.IsCompletelyOverridesRendering)
		{
			OnMainRendering(frameData);
			OnAfterRendering(context, frameData);
		}
	}

	private void OnBeforeRendering(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData renderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghShadowData waaaghShadowData = frameData.Get<WaaaghShadowData>();
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererEnqueuePasses)))
		{
			EnqueuePass(m_InitializeRenderStatePass);
			EnqueuePass(m_SetupProbeVolumesPass);
			EnqueuePass(m_DitheringSetupPass);
			EnqueuePass(m_JitteredDitheringSetupPass);
			EnqueuePass(m_SetupTranslucencyProfilesPass);
			EnqueuePass(m_IRSSubmitPass);
			if (waaaghCameraData.IrsData.Enabled)
			{
				EnqueuePass(m_IRSCullingPass);
			}
		}
		if (base.DebugHandler != null)
		{
			base.DebugHandler.Setup(context, frameData);
			if (base.DebugHandler.IsCompletelyOverridesRendering)
			{
				return;
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupLights)))
		{
			if (waaaghCameraData.IsLightingEnabled)
			{
				m_WaaaghLights.StartSetupJobs(context, frameData, m_Settings.TileSize);
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupFeatures)))
		{
			for (int i = 0; i < base.RendererFeatures.Count; i++)
			{
				base.RendererFeatures[i].StartSetupJobs(frameData);
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererEnqueuePasses)))
		{
			RTHandles.SetReferenceSize(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
			bool flag = waaaghShadowData.ShadowQuality != 0 && waaaghCameraData.IsLightingEnabled && waaaghCameraData.maxShadowDistance > 0f;
			EnqueueGPUDrivenPassesOnBeforeRendering(renderingData, flag);
			if (flag)
			{
				EnqueuePass(m_NativeShadowCasterPass);
			}
		}
	}

	public void EnqueueGPUDrivenPassesOnBeforeRendering(WaaaghRenderingData renderingData, bool shadows)
	{
		if (renderingData.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			EnqueuePass(m_GPUDrivenCullingPreparePass);
			EnqueuePass(m_GPUDrivenShadowCullingPass);
		}
	}

	public void EnqueueClearGBufferPass()
	{
		EnqueuePass(m_ClearGBufferPass);
	}

	private void OnMainRendering(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupVFX)))
		{
			SetupVFXCameraBuffer(waaaghCameraData);
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererEnqueuePasses)))
		{
			bool num = waaaghCameraData.postProcessEnabled && m_PostProcessPasses.IsCreated;
			bool flag = waaaghCameraData.postProcessEnabled && m_PostProcessPasses.IsCreated;
			bool flag2 = waaaghCameraData.renderType == CameraRenderType.Base || !waaaghCameraData.clearDepth || waaaghCameraData.IsLightingEnabled || waaaghCameraData.IrsData.IrsHasOpaques || waaaghCameraData.IrsData.IrsHasOpaqueDistortions;
			waaaghCameraData.IsTemporalAAEnabled();
			if (waaaghCameraData.postProcessEnabled)
			{
				_ = m_PostProcessPasses.IsCreated;
			}
			else
				_ = 0;
			EnqueuePass(m_ClearPass);
			EnqueueClearGBufferPass();
			EnqueuePass(m_DepthPrePass);
			EnqueueGPUDrivenPassesOnMainRendering(waaaghRenderingData);
			EnqueuePass(m_GBufferPass);
			EnqueuePass(m_TerrainPass);
			if (flag2)
			{
				EnqueuePass(m_CopyDepthAfterGBufferPass);
				if (waaaghCameraData.IsLightingEnabled)
				{
					EnqueuePass(m_ComputeTilesMinMaxZPass);
				}
			}
			EnqueuePass(m_MotionVectorsPass);
			EnqueuePass(m_DrawDecalsPass);
			if (waaaghCameraData.IsLightingEnabled)
			{
				EnqueuePass(m_SetupLightDataPass);
				EnqueuePass(m_LightCullingPass);
				if (m_Settings.DeferredLightingMode == DeferredLightingMode.Compute)
				{
					EnqueuePass(m_DeferredLightingBuildVariantsPass);
					EnqueuePass(m_DeferredLightingComputePass);
				}
				else
				{
					EnqueuePass(m_DeferredLightingPass);
				}
				EnqueuePass(m_SetupReflectionProbesPass);
				if (waaaghRenderingData.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
				{
					EnqueuePass(m_GPUDrivenForwardReflectionProbesPass);
				}
			}
			if (num)
			{
				EnqueuePass(m_PostProcessPasses.BeforeTransparentPostProcessPass);
			}
			EnqueuePass(m_DrawObjectsWithUnsupportedMaterials);
			EnqueuePass(m_DrawObjectsWithMissingMaterials);
			if (flag)
			{
				EnqueuePass(m_PostProcessPasses.ColorGradingLutPass);
			}
			if (waaaghCameraData.camera.clearFlags == CameraClearFlags.Skybox && waaaghCameraData.renderType != CameraRenderType.Overlay && (RenderSettings.skybox != null || (waaaghCameraData.camera.TryGetComponent<Skybox>(out var component) && component.material != null)))
			{
				EnqueuePass(m_DrawSkyboxPass);
			}
			EnqueuePass(m_DrawColorPyramidAfterOpaquePass);
			EnqueuePass(m_DepthDistortionPrePass);
			EnqueuePass(m_GBufferDistortionPass);
			EnqueuePass(m_DrawOpaqueDistortionPass);
			if (flag2)
			{
				EnqueuePass(m_CopyDepthAfterOpaqueDistortion);
			}
			if (waaaghCameraData.IsLightingEnabled)
			{
				if (waaaghCameraData.IsSSREnabled)
				{
					if (waaaghCameraData.IsDepthPyramidNeed)
					{
						EnqueuePass(m_DepthPyramidPass);
					}
					EnqueuePass(m_ScreenSpaceReflectionsPass);
				}
				EnqueuePass(m_DeferredReflectionsPass);
			}
			if (waaaghCameraData.IsFogEnabled)
			{
				EnqueuePass(m_FogPass);
			}
			EnqueuePass(m_DrawTransparentPass);
			EnqueuePass(m_DrawColorPyramidAfterTransparentPass);
			EnqueuePass(m_DrawDistortionVectorsPass);
			EnqueuePass(m_RawHistoryPass);
			EnqueuePass(m_RenderOverlayForwardPass);
			EnqueuePass(m_DrawGUIDecalsPass);
		}
	}

	public void EnqueueGPUDrivenPassesOnMainRendering(WaaaghRenderingData renderingData)
	{
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = renderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenBRGSettings settings = gPUDrivenBatchRendererGroup.Settings;
		if (gPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			GPUDrivenCullingPass pass = (settings.OcclusionCulling ? m_GPUDrivenMainFirstCullingPass : m_GPUDrivenMainCullingPass);
			EnqueuePass(pass);
		}
		if (gPUDrivenBatchRendererGroup.IsEnabledAndInitialized && settings != null && settings.OcclusionCulling)
		{
			if (settings.DepthReprojection)
			{
				EnqueuePass(m_GPUDrivenCullingDepthReprojectionPass);
			}
			EnqueuePass(m_GPUDrivenHDBPass);
			EnqueuePass(m_GPUDrivenMainFalseNegativeCullingPass);
			if (settings.DepthReprojection)
			{
				EnqueuePass(m_GPUDrivenCullingDepthHistoryPass);
			}
		}
	}

	private void OnAfterRendering(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghPostProcessingData waaaghPostProcessingData = frameData.Get<WaaaghPostProcessingData>();
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupFeaturesComplete)))
		{
			for (int i = 0; i < base.RendererFeatures.Count; i++)
			{
				base.RendererFeatures[i].CompleteSetupJobs();
			}
			for (int j = 0; j < base.RendererFeatures.Count; j++)
			{
				base.RendererFeatures[j].AddRenderPasses(this, frameData);
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSetupLightsComplete)))
		{
			if (waaaghCameraData.IsLightingEnabled)
			{
				m_WaaaghLights.CompleteSetupJobs(context, frameData);
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererEnqueuePasses)))
		{
			bool num = waaaghCameraData.postProcessEnabled && m_PostProcessPasses.IsCreated;
			if (waaaghPostProcessingData.isEnabled)
			{
				_ = m_PostProcessPasses.IsCreated;
			}
			else
				_ = 0;
			bool flag = num && (waaaghCameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing || (waaaghCameraData.imageScalingMode == ImageScalingMode.Upscaling && waaaghCameraData.upscalingFilter != 0) || (waaaghCameraData.IsTemporalAAEnabled() && waaaghCameraData.taaSettings.contrastAdaptiveSharpening > 0f));
			bool flag2 = (GetOverlayUISubset() & UISubset.UIToolkit_UGUI) != 0 && waaaghCameraData.rendersOverlayUI;
			bool flag3 = (GetOverlayUISubset() & UISubset.LowLevel) != 0 && waaaghCameraData.rendersOverlayUI;
			if (num)
			{
				m_PostProcessPasses.PostProcessPass.HasFinalPass = flag;
				EnqueuePass(m_PostProcessPasses.PostProcessPass);
			}
			if (flag)
			{
				EnqueuePass(m_PostProcessPasses.FinalPostProcessPass);
			}
			if (waaaghCameraData.CameraResolveRequired)
			{
				if (flag2)
				{
					EnqueuePass(m_DrawUGUIOverlayPass);
				}
				EnqueuePass(m_FinalBlitPass);
			}
			if (flag3)
			{
				EnqueuePass(m_DrawIMGUIOverlayPass);
			}
			bool flag4 = waaaghCameraData.TargetDepthTexture != null;
			if (waaaghCameraData.CameraResolveRequired && waaaghCameraData.CameraResolveTargetBufferType == CameraResolveTargetType.Backbuffer && flag4)
			{
				EnqueuePass(m_FinalCopyDepthPass);
			}
		}
	}

	protected override void CreateResources(ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		waaaghResourceData.LightDataConstantBuffer = renderGraph.ImportBuffer(m_WaaaghLights.LightDataConstantBuffer);
		waaaghResourceData.LightVolumeDataConstantBuffer = renderGraph.ImportBuffer(m_WaaaghLights.LightVolumeDataConstantBuffer);
		waaaghResourceData.ZBinsConstantBuffer = renderGraph.ImportBuffer(m_WaaaghLights.ZBinsConstantBuffer);
		waaaghResourceData.LightTilesBuffer = renderGraph.ImportBuffer(m_WaaaghLights.LightTilesBuffer);
		waaaghResourceData.DeferredLightingFeatureTilesBuffer = renderGraph.ImportBuffer(m_WaaaghLights.DeferredLightingFeatureTilesBuffer);
		waaaghResourceData.DeferredLightingFeatureTilesListsBuffer = renderGraph.ImportBuffer(m_WaaaghLights.DeferredLightingFeatureTilesListsBuffer);
		waaaghResourceData.DeferredLightingIndirectArgsBuffer = renderGraph.ImportBuffer(m_WaaaghLights.DeferredLightingIndirectArgsBuffer);
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, WaaaghCameraData cameraData)
	{
		bool num = WaaaghPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(cameraData.maxShadowDistance, 0f);
		bool flag2 = !cameraData.IsLightingEnabled;
		cullingParameters.cullingOptions |= CullingOptions.NeedsReflectionProbes;
		if (num || flag || flag2)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
		}
		if (flag2)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.NeedsLighting;
		}
		cullingParameters.cullingOptions &= ~CullingOptions.OcclusionCull;
		cullingParameters.shadowDistance = cameraData.maxShadowDistance;
	}

	private void SetupVFXCameraBuffer(WaaaghCameraData cameraData)
	{
		if (cameraData != null && cameraData.historyManager != null)
		{
			VFXCameraBufferTypes vFXCameraBufferTypes = VFXManager.IsCameraBufferNeeded(cameraData.camera);
			if (vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Color))
			{
				cameraData.historyManager.RequestAccess<RawColorHistory>();
				RTHandle rTHandle = cameraData.historyManager.GetHistoryForRead<RawColorHistory>()?.GetCurrentTexture();
				VFXManager.SetCameraBuffer(cameraData.camera, VFXCameraBufferTypes.Color, rTHandle, 0, 0, (int)((float)cameraData.pixelWidth * cameraData.renderScale), (int)((float)cameraData.pixelHeight * cameraData.renderScale));
			}
			if (vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Depth))
			{
				cameraData.historyManager.RequestAccess<RawDepthHistory>();
				RTHandle rTHandle2 = cameraData.historyManager.GetHistoryForRead<RawDepthHistory>()?.GetCurrentTexture();
				VFXManager.SetCameraBuffer(cameraData.camera, VFXCameraBufferTypes.Depth, rTHandle2, 0, 0, (int)((float)cameraData.pixelWidth * cameraData.renderScale), (int)((float)cameraData.pixelHeight * cameraData.renderScale));
			}
		}
	}

	private static ComputeBuffer CreateDummyComputeBuffer()
	{
		ComputeBuffer obj = new ComputeBuffer(1, 64, ComputeBufferType.Structured)
		{
			name = "Dummy"
		};
		NativeArray<float> data = new NativeArray<float>(16, Allocator.Temp);
		obj.SetData(data);
		data.Dispose();
		return obj;
	}

	private static bool SystemSupportsDxcCompiler()
	{
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		if (graphicsDeviceType == GraphicsDeviceType.Direct3D12 || (uint)(graphicsDeviceType - 23) <= 2u)
		{
			return true;
		}
		return false;
	}

	protected internal override bool SupportsMotionVectors()
	{
		return true;
	}

	private UISubset GetOverlayUISubset()
	{
		if (OverlayUIMask.HasValue)
		{
			return m_Settings.OverlayUIMask & OverlayUIMask.Value;
		}
		return m_Settings.OverlayUIMask;
	}

	public static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
	{
		TextureDesc desc2 = new TextureDesc(desc.width, desc.height);
		desc2.dimension = desc.dimension;
		desc2.clearBuffer = clear;
		desc2.bindTextureMS = desc.bindMS;
		desc2.colorFormat = desc.graphicsFormat;
		desc2.depthBufferBits = (DepthBits)desc.depthBufferBits;
		desc2.slices = desc.volumeDepth;
		desc2.msaaSamples = (MSAASamples)desc.msaaSamples;
		desc2.name = name;
		desc2.enableRandomWrite = desc.enableRandomWrite;
		desc2.filterMode = filterMode;
		desc2.wrapMode = wrapMode;
		desc2.isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None && desc.depthStencilFormat != GraphicsFormat.None;
		desc2.vrUsage = desc.vrUsage;
		return renderGraph.CreateTexture(in desc2);
	}
}
