using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class UIOverlay
{
	private class UGUIPassData
	{
		public RendererListHandle RendererList;
	}

	private class IMGUIPassData
	{
		public RendererListHandle RendererList;

		public TextureHandle ColorTarget;
	}

	internal static bool ShouldDrawUGUI(UISubset uiSubset, WaaaghCameraData cameraData)
	{
		if ((uiSubset & UISubset.UIToolkit_UGUI) != 0 && cameraData.rendersOverlayUI)
		{
			return cameraData.resolveToScreen;
		}
		return false;
	}

	public static void DrawUGUI(in RecordContext context)
	{
		UGUIPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<UGUIPassData>("UIOverlay.UGUI", out passData, WaaaghProfileId.UIOverlayUGUI.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\UIOverlay.cs", 25);
		UGUIPassData uGUIPassData = passData;
		RenderGraph renderGraph = context.RenderGraph;
		ref Camera camera = ref context.CameraData.camera;
		UISubset uiSubset = UISubset.UIToolkit_UGUI;
		uGUIPassData.RendererList = renderGraph.CreateUIOverlayRendererList(in camera, in uiSubset);
		rasterRenderGraphBuilder.UseRendererList(in passData.RendererList);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(UGUIPassData data, RasterGraphContext context)
		{
			context.cmd.DrawRendererList(data.RendererList);
		});
	}

	public static bool ShouldDrawIMGUI(UISubset uiSubset, WaaaghCameraData cameraData)
	{
		if ((uiSubset & UISubset.LowLevel) != 0)
		{
			return cameraData.rendersOverlayUI;
		}
		return false;
	}

	public static void DrawIMGUI(in RecordContext context)
	{
		IMGUIPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<IMGUIPassData>("UIOverlay.IMGUI", out passData, WaaaghProfileId.UIOverlayIMGUI.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\UIOverlay.cs", 57);
		passData.ColorTarget = context.FrameResources.FinalTarget.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.ColorTarget, AccessFlags.Write);
		IMGUIPassData iMGUIPassData = passData;
		RenderGraph renderGraph = context.RenderGraph;
		ref Camera camera = ref context.CameraData.camera;
		UISubset uiSubset = UISubset.LowLevel;
		iMGUIPassData.RendererList = renderGraph.CreateUIOverlayRendererList(in camera, in uiSubset);
		unsafeRenderGraphBuilder.UseRendererList(in passData.RendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(IMGUIPassData data, UnsafeGraphContext context)
		{
			context.cmd.SetRenderTarget(data.ColorTarget);
			context.cmd.DrawRendererList(data.RendererList);
		});
	}
}
