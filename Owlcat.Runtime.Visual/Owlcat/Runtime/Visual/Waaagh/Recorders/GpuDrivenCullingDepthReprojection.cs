using Owlcat.Runtime.Visual.GPUDrivenBRG;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDrivenCullingDepthReprojection
{
	private sealed class PassData
	{
		public TextureHandle PackedReprojectedDepth;

		public GPUDrivenDepthReprojectionUtils.ReprojectionParameters ReprojectionParameters;

		public TextureHandle Source;
	}

	public static void Record(in RecordContext context, GPUDrivenDepthReprojectionUtils depthReprojectionUtils, out TextureHandle packedReprojectedDepth, out GPUDrivenDepthReprojectionUtils.ReprojectionParameters depthReprojectionParameters)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("GpuDriven.ReprojectDepth", out passData, WaaaghProfileId.GpuDrivenDepthReprojection.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\GpuDriven\\GpuDrivenCullingDepthReprojection.cs", 15);
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(context.CameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true);
		Matrix4x4 viewMatrix = context.CameraData.GetViewMatrix();
		Matrix4x4 gpuViewProjection = gPUProjectionMatrix * viewMatrix;
		passData.ReprojectionParameters = depthReprojectionUtils.Setup(context.RenderGraph, context.CameraData, context.GPUDrivenBatchRendererGroup, gpuViewProjection, out passData.Source);
		if (!passData.ReprojectionParameters.Cull)
		{
			passData.PackedReprojectedDepth = context.RenderGraph.CreateTexture(in passData.ReprojectionParameters.PackedReprojectedDepthDesc);
			unsafeRenderGraphBuilder.UseTexture(in passData.PackedReprojectedDepth, AccessFlags.Write);
			unsafeRenderGraphBuilder.UseTexture(in passData.Source);
		}
		else
		{
			passData.PackedReprojectedDepth = TextureHandle.nullHandle;
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			if (!data.ReprojectionParameters.Cull)
			{
				GPUDrivenDepthReprojectionUtils.Reproject(in data.ReprojectionParameters, context.cmd, data.Source, data.PackedReprojectedDepth);
			}
		});
		packedReprojectedDepth = passData.PackedReprojectedDepth;
		depthReprojectionParameters = passData.ReprojectionParameters;
	}
}
