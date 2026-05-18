using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public static class MapOverlays
{
	private class MapOverlayPassData
	{
		public TextureHandle OverlayHandle;

		public float Size;
	}

	public static void Record(in RecordContext context)
	{
		switch (context.DebugContext.DebugData.RenderingDebug.DebugMapOverlay)
		{
		case DebugMapOverlay.MotionVectors:
			DrawMotionVectors(in context);
			break;
		case DebugMapOverlay.Depth:
			DrawDepth(context);
			break;
		case DebugMapOverlay.None:
			break;
		}
	}

	private static void DrawMotionVectors(in RecordContext context)
	{
		MapOverlayPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<MapOverlayPassData>("DEBUG - MotionVectors Overlay", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\MapOverlays.cs", 39);
		passData.OverlayHandle = context.FrameResources.CameraAdditionalTargets.MotionVectors;
		unsafeRenderGraphBuilder.UseTexture(in passData.OverlayHandle);
		passData.Size = context.DebugContext.DebugData.RenderingDebug.MapSize;
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderFunc<MapOverlayPassData>(ExecturePass);
	}

	private static void DrawDepth(RecordContext context)
	{
		MapOverlayPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<MapOverlayPassData>("DEBUG - Depth Overlay", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\MapOverlays.cs", 52);
		passData.OverlayHandle = context.FrameResources.CameraStackTargets.Depth;
		unsafeRenderGraphBuilder.UseTexture(in passData.OverlayHandle);
		passData.Size = context.DebugContext.DebugData.RenderingDebug.MapSize;
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderFunc<MapOverlayPassData>(ExecturePass);
	}

	private static void ExecturePass(MapOverlayPassData data, UnsafeGraphContext context)
	{
		if (data.OverlayHandle.IsValid())
		{
			Vector4 scaleBiasTex = new Vector4(1f, 1f, 0f, 0f);
			bool flag = false;
			Vector4 scaleBiasRT = new Vector4(data.Size, data.Size, 0f, flag ? (1f - data.Size) : 0f);
			Blitter.BlitQuad(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), data.OverlayHandle, scaleBiasTex, scaleBiasRT, 0, bilinear: false);
		}
	}
}
