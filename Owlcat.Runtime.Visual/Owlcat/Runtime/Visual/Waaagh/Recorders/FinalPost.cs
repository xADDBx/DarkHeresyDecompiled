using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class FinalPost
{
	private class PostProcessingFinalSetupPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal WaaaghCameraData cameraData;
	}

	private class PostProcessingFinalBlitPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal Material material;

		internal WaaaghCameraData cameraData;

		internal FinalBlitSettings settings;
	}

	public static void RenderFinalSetup(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle destination, ref FinalBlitSettings settings)
	{
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		PostProcessingFinalSetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalSetupPassData>("Postprocessing Final Setup Pass", out passData, WaaaghProfileId.FinalSetup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\FinalPost.cs", 25))
		{
			Material scalingSetup = processor.MatLib.ScalingSetup;
			if (settings.isFxaaEnabled)
			{
				scalingSetup.EnableKeyword(ShaderKeywordStrings.Fxaa);
			}
			if (settings.isFsrEnabled)
			{
				scalingSetup.EnableKeyword(settings.hdrOperations.HasFlag(HDROutputUtils.Operation.ColorEncoding) ? "_GAMMA_20_AND_HDR_INPUT" : "_GAMMA_20");
			}
			if (settings.isAlphaOutputEnabled)
			{
				CoreUtils.SetKeyword(scalingSetup, "_ENABLE_ALPHA_OUTPUT", settings.isAlphaOutputEnabled);
			}
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.destinationTexture = destination;
			rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.cameraData = cameraData;
			passData.material = scalingSetup;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalSetupPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				RTHandle rTHandle = data.sourceTexture;
				PostProcessUtils.SetSourceSize(cmd, rTHandle);
				PostProcessor.ScaleViewportAndBlit(context.cmd, rTHandle, data.destinationTexture, data.cameraData, data.material);
			});
		}
		processor.CameraStackTargets.SetCurrentPostProcessSource(destination);
	}

	public static void RenderFinalBlit(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle postProcessingTarget, ref FinalBlitSettings settings)
	{
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		PostProcessingFinalBlitPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PostProcessingFinalBlitPassData>("Postprocessing Final Blit Pass", out passData, WaaaghProfileId.FinalPostBlit.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\FinalPost.cs", 76);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.destinationTexture = postProcessingTarget;
		rasterRenderGraphBuilder.SetRenderAttachment(postProcessingTarget, 0);
		passData.sourceTexture = input;
		rasterRenderGraphBuilder.UseTexture(in input);
		passData.cameraData = cameraData;
		passData.material = processor.MatLib.FinalPass;
		passData.settings = settings;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PostProcessingFinalBlitPassData data, RasterGraphContext context)
		{
			RasterCommandBuffer cmd = context.cmd;
			Material material = data.material;
			bool isFxaaEnabled = data.settings.isFxaaEnabled;
			bool isFsrEnabled = data.settings.isFsrEnabled;
			bool isTaaSharpeningEnabled = data.settings.isTaaSharpeningEnabled;
			bool requireHDROutput = data.settings.requireHDROutput;
			bool resolveToDebugScreen = data.settings.resolveToDebugScreen;
			bool isAlphaOutputEnabled = data.settings.isAlphaOutputEnabled;
			RTHandle rTHandle = data.sourceTexture;
			_ = (RTHandle)data.destinationTexture;
			PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
			if (isFxaaEnabled)
			{
				material.EnableKeyword(ShaderKeywordStrings.Fxaa);
			}
			if (isFsrEnabled)
			{
				float sharpnessLinear = (data.cameraData.fsrOverrideSharpness ? data.cameraData.fsrSharpness : 0.92f);
				if (data.cameraData.fsrSharpness > 0f)
				{
					material.EnableKeyword(requireHDROutput ? "_EASU_RCAS_AND_HDR_INPUT" : "_RCAS");
					FSRUtils.SetRcasConstantsLinear(cmd, sharpnessLinear);
				}
			}
			else if (isTaaSharpeningEnabled)
			{
				material.EnableKeyword("_RCAS");
				FSRUtils.SetRcasConstantsLinear(cmd, data.cameraData.taaSettings.contrastAdaptiveSharpening);
			}
			if (isAlphaOutputEnabled)
			{
				CoreUtils.SetKeyword(material, "_ENABLE_ALPHA_OUTPUT", isAlphaOutputEnabled);
			}
			int num = 0 & ((!resolveToDebugScreen) ? 1 : 0);
			Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			Vector4 scaleBias = ((num != 0 && data.cameraData.targetTexture == null && SystemInfo.graphicsUVStartsAtTop) ? new Vector4(vector.x, 0f - vector.y, 0f, vector.y) : new Vector4(vector.x, vector.y, 0f, 0f));
			cmd.SetViewport(data.cameraData.pixelRect);
			Blitter.BlitTexture(cmd, rTHandle, scaleBias, material, 0);
		});
	}
}
