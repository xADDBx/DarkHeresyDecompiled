using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class IRS
{
	private class CullingPassData
	{
		public Camera Camera;
	}

	public static void CullingPass(in RendererRecordContext context)
	{
		CullingPass(context.RenderGraph, context.CameraData);
	}

	public static void CullingPass(in RecordContext context)
	{
		CullingPass(context.RenderGraph, context.CameraData);
	}

	public static void CullingPass(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		CullingPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<CullingPassData>("IRS.CullingPass", out passData, WaaaghProfileId.IRSCullingPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\IRS.cs", 23);
		passData.Camera = cameraData.camera;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(CullingPassData data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			IndirectRenderingSystem.Instance.Cull(nativeCommandBuffer, data.Camera);
		});
	}

	internal static bool ShouldRender(in RendererRecordContext context)
	{
		return ShouldRender(context.CameraData);
	}

	internal static bool ShouldRender(in RecordContext context)
	{
		return ShouldRender(context.CameraData);
	}

	internal static bool ShouldRender(WaaaghCameraData cameraData)
	{
		return cameraData.IrsData.Enabled;
	}
}
