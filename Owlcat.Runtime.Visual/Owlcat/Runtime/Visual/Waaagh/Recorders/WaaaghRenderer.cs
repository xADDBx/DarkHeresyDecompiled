using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class WaaaghRenderer : IPipelineRenderer, IDisposable
{
	private readonly WaaaghRendererData m_Settings;

	private readonly WaaaghLights m_Lights;

	private readonly WaaaghReflectionProbes m_ReflectionProbes;

	private readonly DeferredReflectionProbeBatcher m_ReflectionProbeBatcher;

	private readonly MaterialLibrary m_MaterialLibrary;

	private readonly FinalTargetHandles m_CurrentFinalTargets;

	private readonly PostProcessor m_PostProcessor;

	private readonly RendererFeatureExtensions m_Extensions;

	private readonly DepthPyramidGenerationUtils m_DepthPyramidGenerationUtils;

	private readonly GPUDrivenDepthReprojectionUtils m_DepthReprojectionUtils;

	private readonly RenderRuntimeTextures m_RuntimeTextures;

	public UISubset? OverlayUIMask;

	public WaaaghRenderer(WaaaghRendererData settings)
	{
		m_Settings = settings;
		m_Lights = new WaaaghLights();
		m_ReflectionProbes = new WaaaghReflectionProbes();
		m_ReflectionProbeBatcher = new DeferredReflectionProbeBatcher(m_ReflectionProbes, settings);
		m_MaterialLibrary = new MaterialLibrary(settings.Shaders);
		m_CurrentFinalTargets = new FinalTargetHandles();
		m_PostProcessor = new PostProcessor(settings.PostProcessResources, WaaaghPipeline.Asset.PostProcessSettings, m_MaterialLibrary.BlitMaterial);
		m_Extensions = new RendererFeatureExtensions(settings.RendererFeatures);
		m_DepthPyramidGenerationUtils = new DepthPyramidGenerationUtils(settings);
		m_DepthReprojectionUtils = new GPUDrivenDepthReprojectionUtils(m_MaterialLibrary.CopyDepth);
		m_RuntimeTextures = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
	}

	public void Dispose()
	{
		m_Extensions.Dispose();
		m_MaterialLibrary?.Dispose();
		m_CurrentFinalTargets?.Dispose();
		m_PostProcessor?.Dispose();
		m_ReflectionProbeBatcher.Dispose();
		m_ReflectionProbes.Dispose();
		m_Lights.Dispose();
	}

	public void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, WaaaghCameraData cameraData)
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

	public void Setup(in RendererSetupContext genericContext)
	{
		SetupContext setupContext = default(SetupContext);
		setupContext.ScriptableRenderContext = genericContext.ScriptableRenderContext;
		setupContext.CameraData = genericContext.CameraData;
		setupContext.RenderingData = genericContext.RenderingData;
		setupContext.ShadowData = genericContext.ShadowData;
		setupContext.Lights = m_Lights;
		SetupContext context = setupContext;
		Setup(in context);
	}

	private void Setup(in SetupContext context)
	{
		m_Lights.StartSetupJobs(context.ScriptableRenderContext, context.CameraData, context.RenderingData, context.ShadowData, m_Settings.TileSize);
		m_Lights.CompleteSetupJobs(context.ScriptableRenderContext, context.CameraData, context.RenderingData, context.ShadowData);
		UpdateCameraHistory(context.CameraData);
		JobHandle jobHandle = m_Extensions.ScheduleSetupJobs(in context, default(JobHandle));
		m_ReflectionProbes.PrepareCpuData(ref context.RenderingData.CullResults);
		m_PostProcessor.Setup(in context);
		m_Extensions.Setup(in context);
		jobHandle.Complete();
		m_Extensions.CompleteSetupJobs(in context);
	}

	public void Record(in RendererRecordContext genericContext)
	{
		FrameResources frameResources = FrameResourcesFactory.Create(genericContext.RenderGraph, genericContext.RenderingData, genericContext.CameraData, genericContext.VirtualTextureManager, m_Lights, genericContext.ShadowData, m_CurrentFinalTargets);
		RecordContext recordContext = default(RecordContext);
		recordContext.RenderGraph = genericContext.RenderGraph;
		recordContext.Lights = m_Lights;
		recordContext.RenderingData = genericContext.RenderingData;
		recordContext.CameraData = genericContext.CameraData;
		recordContext.FrameResources = frameResources;
		recordContext.MaterialLibrary = m_MaterialLibrary;
		recordContext.Shaders = m_Settings.Shaders;
		recordContext.Textures = m_RuntimeTextures;
		recordContext.VirtualTextureManager = genericContext.VirtualTextureManager;
		recordContext.ShadowData = genericContext.ShadowData;
		recordContext.GPUDrivenBatchRendererGroup = genericContext.GPUDrivenBatchRendererGroup;
		recordContext.DebugContext = genericContext.DebugContext;
		recordContext.ReflectionProbes = m_ReflectionProbes;
		recordContext.DeferredReflectionProbeBatcher = m_ReflectionProbeBatcher;
		RecordContext context = recordContext;
		Record(in context);
	}

	private void Record(in RecordContext context)
	{
		GlobalStateRecorder.InitializeRenderStatePass(in context);
		GlobalStateRecorder.SetupFogPass(in context);
		APV.SetupProbeVolumesPass(in context);
		Dithering.SetupDitheringPass(in context, allowJitter: false);
		if (GpuDriven.IsGpuDrivenEnabled(in context))
		{
			GpuDriven.PrepareCulling(in context);
			GpuDriven.CullShadows(in context);
			if (!GpuDriven.IsOcclusionCullingEnabled(in context))
			{
				GpuDriven.Cull(in context);
			}
		}
		if (ShadowsDrawer.ShouldDraw(in context))
		{
			ShadowsDrawer.ShadowCasterPass(in context);
		}
		if (IRS.ShouldRender(in context))
		{
			IRS.CullingPass(in context);
		}
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeRendering);
		CameraRecorder.SetupCamera(in context, noJitter: false);
		CameraRecorder.VFXPrepareCameraPass(in context);
		ClearPasses.ClearCameraTargets(in context);
		Dithering.SetupDitheringPass(in context, allowJitter: true);
		if (GpuDriven.IsGpuDrivenEnabled(in context) && GpuDriven.IsOcclusionCullingEnabled(in context))
		{
			GpuDriven.CullOcclusionCoarse(in context);
		}
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeDrawDepthPrePass);
		DeferredOpaque.DrawDepthPrePass(in context);
		if (GpuDriven.IsGpuDrivenEnabled(in context) && GpuDriven.IsOcclusionCullingEnabled(in context))
		{
			if (GpuDriven.IsDepthDeprojectionEnabled(in context))
			{
				GpuDriven.ReprojectDepth(in context, m_DepthReprojectionUtils, out var packedReprojectedDepth, out var depthReprojectionParameters);
				DepthPyramid.BuildDepthPyramid(in context, m_DepthPyramidGenerationUtils, useMax: true, packedReprojectedDepth, depthReprojectionParameters);
			}
			else
			{
				DepthPyramid.BuildDepthPyramid(in context, m_DepthPyramidGenerationUtils, useMax: true, TextureHandle.nullHandle, default(GPUDrivenDepthReprojectionUtils.ReprojectionParameters));
			}
			GpuDriven.CullOcclusionFine(in context);
			if (GpuDriven.IsDepthDeprojectionEnabled(in context))
			{
				GpuDriven.UpdateCullingDepthHistory(in context, m_DepthPyramidGenerationUtils);
			}
		}
		DeferredOpaque.DrawGBufferPass(in context);
		context.VirtualTextureManager?.ResolveFeedback(in context);
		CopyPasses.CopyDepthToDepthCopy(in context);
		MotionVectors.MotionVectorsPass(in context);
		context.RenderGraph.BeginProfilingSampler(Decals.DeferredDecalsProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\WaaaghRenderer.cs", 262);
		Decals.InitializeDBuffer(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeDrawDeferredDecals);
		Decals.DrawDeferredDecals(in context);
		context.VirtualTextureManager?.ResolveFeedback(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.AfterDrawDeferredDecals);
		Decals.ResolveDBuffer(in context);
		context.RenderGraph.EndProfilingSampler(Decals.DeferredDecalsProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\WaaaghRenderer.cs", 270);
		if (DeferredLighting.ShouldApplyLighting(in context))
		{
			LightCookieManager.UpdateLightCookiePass(in context);
			DeferredLighting.ComputeTilesMinMaxZPass(in context);
			DeferredLighting.SetupLightDataPass(in context);
			DeferredLighting.LightCullingPass(in context);
			m_Extensions.Record(in context, RecordExtensionPoint.BeforeDeferredLighting);
			if (m_Settings.DeferredLightingMode == DeferredLightingMode.Draw)
			{
				DeferredLighting.LightingPassRaster(in context);
			}
			else
			{
				DeferredLighting.BuildVariantsPass(in context);
				DeferredLighting.LightingPassCompute(in context);
			}
			m_Extensions.Record(in context, RecordExtensionPoint.AfterDeferredLighting);
		}
		Validation.DrawObjectWithInvalidMaterials(in context);
		if (SkyBoxDrawer.ShouldDrawSkybox(in context))
		{
			SkyBoxDrawer.DrawSkybox(in context);
		}
		ColorPyramid.BuildColorPyramid(in context);
		Reflections.SetupReflectionProbesPass(in context);
		Distortion.DrawOpaqueDepth(in context);
		Distortion.DrawOpaqueGBuffer(in context);
		Distortion.DrawOpaqueColor(in context);
		CopyPasses.CopyDepthToDepthCopy(in context);
		if (DeferredLighting.ShouldApplyLighting(in context))
		{
			if (SSR.ShouldDraw(in context))
			{
				DepthPyramid.BuildDepthPyramid(in context, m_DepthPyramidGenerationUtils);
				SSR.Draw(in context);
			}
			Reflections.DeferredReflectionsPass(in context);
			if (GpuDriven.IsGpuDrivenEnabled(in context))
			{
				GpuDriven.UpdateForwardReflectionProbes(in context);
			}
		}
		DeferredLighting.DeferredFogPass(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeDrawTransparent1);
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeDrawTransparent2);
		Transparent.DrawObjects(in context);
		context.VirtualTextureManager?.ResolveFeedback(in context);
		ColorPyramid.BuildColorPyramid(in context);
		Distortion.DrawTransparentObjects(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.AfterDrawTransparent);
		HistoryPasses.RecordRawHistoryPasses(in context);
		Overlay.DrawObjects(in context);
		GizmosDrawer.DrawGizmosPreImageEffects(in context);
		Decals.DrawForwardDecals(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.BeforeDrawPostProcess);
		m_PostProcessor.Record(in context);
		m_Extensions.Record(in context, RecordExtensionPoint.AfterDrawPostProcess);
		m_PostProcessor.FinalizeToTarget(in context);
		if (UIOverlay.ShouldDrawUGUI(GetOverlayUISubset(), context.CameraData))
		{
			UIOverlay.DrawUGUI(in context);
		}
		m_Extensions.Record(in context, RecordExtensionPoint.AfterRendering);
		if (context.CameraData.StackInfo.IsLastCamera)
		{
			CopyPasses.CopyColorToFinalTarget(in context);
			if (CopyPasses.ShouldCopyDepthToFinalTarget(in context))
			{
				CopyPasses.CopyDepthToFinalTarget(in context);
			}
			GizmosDrawer.DrawGizmosPostImageEffects(in context);
		}
		if (UIOverlay.ShouldDrawIMGUI(GetOverlayUISubset(), context.CameraData))
		{
			UIOverlay.DrawIMGUI(in context);
		}
	}

	public void Cleanup()
	{
		m_Extensions.Cleanup();
	}

	private void UpdateCameraHistory(WaaaghCameraData cameraData)
	{
		if (cameraData != null && cameraData.historyManager != null)
		{
			int num = 0;
			if (0 == 0 || num == 0)
			{
				WaaaghCameraHistory historyManager = cameraData.historyManager;
				historyManager.GatherHistoryRequests();
				historyManager.ReleaseUnusedHistory();
				historyManager.SwapAndSetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
			}
		}
	}

	private UISubset GetOverlayUISubset()
	{
		if (OverlayUIMask.HasValue)
		{
			return m_Settings.OverlayUIMask & OverlayUIMask.Value;
		}
		return m_Settings.OverlayUIMask;
	}

	public bool SupportsPipelineFeature(PipelineFeature feature)
	{
		return feature switch
		{
			PipelineFeature.GpuDriven => m_Settings.DrawViaGPUDrivenBRG, 
			PipelineFeature.MotionVectors => true, 
			_ => throw new ArgumentOutOfRangeException("feature", feature, null), 
		};
	}

	void IPipelineRenderer.Setup(in RendererSetupContext context)
	{
		Setup(in context);
	}

	void IPipelineRenderer.Record(in RendererRecordContext context)
	{
		Record(in context);
	}
}
