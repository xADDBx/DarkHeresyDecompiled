using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class ClearPasses
{
	private sealed class ClearPassData
	{
		public RTClearFlags ClearFlags;

		public Color ClearColorValue;

		public uint ClearStencilValue;
	}

	public static void ClearCameraTargets(in RecordContext context)
	{
		ClearPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<ClearPassData>("ClearCameraTargets", out passData2, WaaaghProfileId.ClearCameraTargets.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ClearPasses.cs", 20);
		passData2.ClearColorValue = context.CameraData.backgroundColor;
		passData2.ClearColorValue.a = 0f;
		passData2.ClearStencilValue = 128u;
		if (context.CameraData.renderType == CameraRenderType.Base)
		{
			ClearPassData clearPassData = passData2;
			clearPassData.ClearFlags = context.CameraData.camera.clearFlags switch
			{
				CameraClearFlags.Skybox => RTClearFlags.All, 
				CameraClearFlags.Color => RTClearFlags.All, 
				CameraClearFlags.Depth => RTClearFlags.All, 
				CameraClearFlags.Nothing => RTClearFlags.Stencil, 
				_ => RTClearFlags.Stencil, 
			};
		}
		else
		{
			passData2.ClearFlags = (context.CameraData.clearDepth ? RTClearFlags.DepthStencil : RTClearFlags.None);
		}
		if ((passData2.ClearFlags & RTClearFlags.Color) != 0)
		{
			unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0, AccessFlags.WriteAll);
		}
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth, AccessFlags.WriteAll);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(ClearPassData passData, UnsafeGraphContext ctx)
		{
			ctx.cmd.ClearRenderTarget(passData.ClearFlags, passData.ClearColorValue, 1f, passData.ClearStencilValue);
		});
	}
}
