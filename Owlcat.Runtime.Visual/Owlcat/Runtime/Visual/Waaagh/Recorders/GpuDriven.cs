using Owlcat.Runtime.Visual.GPUDrivenBRG;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDriven
{
	public static bool IsGpuDrivenEnabled(in RendererRecordContext context)
	{
		return context.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized;
	}

	public static bool IsGpuDrivenEnabled(in RecordContext context)
	{
		return context.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized;
	}

	public static bool IsOcclusionCullingEnabled(in RecordContext context)
	{
		return context.GPUDrivenBatchRendererGroup.Settings.OcclusionCulling;
	}

	public static bool IsDepthDeprojectionEnabled(in RecordContext context)
	{
		return context.GPUDrivenBatchRendererGroup.Settings.DepthReprojection;
	}

	public static void PrepareCulling(in RendererRecordContext context)
	{
		GpuDrivenCullingPrepare.Record(context.RenderGraph, context.GPUDrivenBatchRendererGroup);
	}

	public static void PrepareCulling(in RecordContext context)
	{
		GpuDrivenCullingPrepare.Record(context.RenderGraph, context.GPUDrivenBatchRendererGroup);
	}

	public static void CullShadows(in RecordContext context)
	{
		GpuDrivenCulling.Record(context.RenderGraph, context.RenderingData, context.CameraData, context.FrameResources.CameraStackTargets.Depth, "GpuDriven.CullShadows", GpuDrivenCulling.OcclusionCullingPassType.None, (BatchCullingViewType vt, GPUDrivenRendererGroupPool.ViewType _) => vt == BatchCullingViewType.Light);
	}

	public static void Cull(in RendererRecordContext context, TextureHandle cameraDepthTexture)
	{
		GpuDrivenCulling.Record(context.RenderGraph, context.RenderingData, context.CameraData, cameraDepthTexture, "GpuDriven.Cull", GpuDrivenCulling.OcclusionCullingPassType.None, (BatchCullingViewType vt, GPUDrivenRendererGroupPool.ViewType _) => vt == BatchCullingViewType.Camera);
	}

	public static void Cull(in RecordContext context)
	{
		GpuDrivenCulling.Record(context.RenderGraph, context.RenderingData, context.CameraData, context.FrameResources.CameraStackTargets.Depth, "GpuDriven.Cull", GpuDrivenCulling.OcclusionCullingPassType.None, (BatchCullingViewType vt, GPUDrivenRendererGroupPool.ViewType _) => vt == BatchCullingViewType.Camera);
	}

	public static void CullOcclusionCoarse(in RecordContext context)
	{
		GpuDrivenCulling.Record(context.RenderGraph, context.RenderingData, context.CameraData, context.FrameResources.CameraStackTargets.Depth, "GpuDriven.CullCoarse", GpuDrivenCulling.OcclusionCullingPassType.First, (BatchCullingViewType _, GPUDrivenRendererGroupPool.ViewType vt) => vt == GPUDrivenRendererGroupPool.ViewType.DepthOnly);
	}

	public static void CullOcclusionFine(in RecordContext context)
	{
		GpuDrivenCulling.Record(context.RenderGraph, context.RenderingData, context.CameraData, context.FrameResources.CameraStackTargets.Depth, "GpuDriven.CullFine", GpuDrivenCulling.OcclusionCullingPassType.FalseNegative, (BatchCullingViewType _, GPUDrivenRendererGroupPool.ViewType vt) => vt == GPUDrivenRendererGroupPool.ViewType.Camera || vt == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors);
	}

	public static void ReprojectDepth(in RecordContext context, GPUDrivenDepthReprojectionUtils depthReprojectionUtils, out TextureHandle packedReprojectedDepth, out GPUDrivenDepthReprojectionUtils.ReprojectionParameters depthReprojectionParameters)
	{
		GpuDrivenCullingDepthReprojection.Record(in context, depthReprojectionUtils, out packedReprojectedDepth, out depthReprojectionParameters);
	}

	public static void UpdateCullingDepthHistory(in RecordContext context, DepthPyramidGenerationUtils depthPyramidGenerationUtils)
	{
		GpuDrivenCullingDepthHistory.Record(in context, depthPyramidGenerationUtils);
	}

	public static void UpdateForwardReflectionProbes(in RecordContext context)
	{
		GpuDrivenForwardReflectionProbes.Record(in context);
	}
}
