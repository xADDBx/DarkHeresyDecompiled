using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class StopNaN
{
	private class StopNaNsPassData
	{
		internal TextureHandle stopNaNTarget;

		internal TextureHandle sourceTexture;

		internal Material stopNaN;
	}

	public static void Render(PostProcessor postProcessor, RenderGraph renderGraph)
	{
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle input = postProcessor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle textureHandle = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_StopNaNsTarget", clear: true, FilterMode.Bilinear);
		StopNaNsPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<StopNaNsPassData>("Stop NaNs", out passData, WaaaghProfileId.StopNaNs.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\StopNaN.cs", 28))
		{
			passData.stopNaNTarget = textureHandle;
			rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0, AccessFlags.ReadWrite);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.stopNaN = postProcessor.MatLib.StopNaN;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(StopNaNsPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				RTHandle rTHandle = data.sourceTexture;
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd, rTHandle, vector, data.stopNaN, 0);
			});
		}
		postProcessor.CameraStackTargets.SetCurrentPostProcessSource(textureHandle);
	}
}
