using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class FSR1Recorder
{
	private class FSR1ScalePassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal bool enableAlphaOutput;
	}

	public static void Render(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle dest, bool enableAlphaOutput)
	{
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		processor.MatLib.Easu.shaderKeywords = null;
		FSR1ScalePassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<FSR1ScalePassData>("Postprocessing Final FSR Scale Pass", out passData, WaaaghProfileId.FinalFSRScale.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\FSR1Recorder.cs", 26))
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.destinationTexture = dest;
			rasterRenderGraphBuilder.SetRenderAttachment(dest, 0);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.material = processor.MatLib.Easu;
			passData.enableAlphaOutput = enableAlphaOutput;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(FSR1ScalePassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				TextureHandle sourceTexture = data.sourceTexture;
				TextureHandle destinationTexture = data.destinationTexture;
				Material material = data.material;
				bool enableAlphaOutput2 = data.enableAlphaOutput;
				RTHandle rTHandle = sourceTexture;
				RTHandle rTHandle2 = destinationTexture;
				Vector2 vector = new Vector2(rTHandle.referenceSize.x, rTHandle.referenceSize.y);
				Vector2 outputImageSizeInPixels = new Vector2(rTHandle2.referenceSize.x, rTHandle2.referenceSize.y);
				FSRUtils.SetEasuConstants(cmd, vector, vector, outputImageSizeInPixels);
				CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", enableAlphaOutput2);
				Vector2 vector2 = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd, rTHandle, vector2, material, 0);
			});
		}
		TextureDesc textureDesc = renderGraph.GetTextureDesc(in dest);
		PostProcessor.UpdateCameraResolution(processor, renderGraph, cameraData, new Vector2Int(textureDesc.width, textureDesc.height));
		processor.CameraStackTargets.SetCurrentPostProcessSource(dest);
	}
}
