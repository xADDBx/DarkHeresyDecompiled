using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawScreenSpaceUIPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RendererListHandle RendererList;
	}

	private class UnsafePassData
	{
		internal RendererListHandle RendererList;

		internal TextureHandle ColorTarget;
	}

	private UISubset m_Subset;

	public override string Name => "DrawScreenSpaceUIPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.DrawScreenSpaceUIPass;

	public DrawScreenSpaceUIPass(UISubset subset, RenderPassEvent evt)
		: base(evt)
	{
		m_Subset = subset;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		TextureHandle input = ((m_Subset == UISubset.UIToolkit_UGUI) ? waaaghResourceData.CameraColorBuffer : waaaghResourceData.CameraResolveColorBuffer);
		TextureHandle tex = ((m_Subset == UISubset.UIToolkit_UGUI) ? waaaghResourceData.CameraDepthBuffer : waaaghResourceData.CameraResolveDepthBuffer);
		renderGraph.BeginProfilingSampler(base.ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\DrawScreenSpaceUIPass.cs", 49);
		if ((m_Subset & UISubset.UIToolkit_UGUI) != 0)
		{
			PassData passData2;
			using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("Draw UGUI Overlay", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\DrawScreenSpaceUIPass.cs", 54);
			rasterRenderGraphBuilder.SetRenderAttachment(input, 0);
			rasterRenderGraphBuilder.SetRenderAttachmentDepth(tex);
			PassData passData3 = passData2;
			ref Camera camera = ref waaaghCameraData.camera;
			UISubset uiSubset = UISubset.UIToolkit_UGUI;
			passData3.RendererList = renderGraph.CreateUIOverlayRendererList(in camera, in uiSubset);
			rasterRenderGraphBuilder.UseRendererList(in passData2.RendererList);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, RasterGraphContext context)
			{
				context.cmd.DrawRendererList(passData.RendererList);
			});
		}
		if ((m_Subset & UISubset.LowLevel) != 0)
		{
			UnsafePassData passData4;
			using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<UnsafePassData>("Draw IMGUI Overlay", out passData4, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\DrawScreenSpaceUIPass.cs", 75);
			passData4.ColorTarget = input;
			unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
			UnsafePassData unsafePassData = passData4;
			ref Camera camera2 = ref waaaghCameraData.camera;
			UISubset uiSubset = UISubset.LowLevel;
			unsafePassData.RendererList = renderGraph.CreateUIOverlayRendererList(in camera2, in uiSubset);
			unsafeRenderGraphBuilder.UseRendererList(in passData4.RendererList);
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(UnsafePassData passData, UnsafeGraphContext context)
			{
				context.cmd.SetRenderTarget(passData.ColorTarget);
				context.cmd.DrawRendererList(passData.RendererList);
			});
		}
		renderGraph.EndProfilingSampler(base.ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\DrawScreenSpaceUIPass.cs", 91);
	}
}
