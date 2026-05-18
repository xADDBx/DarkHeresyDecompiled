using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class STPRecorder
{
	private const string _UpscaledColorTargetName = "_UpscaledColorTargetSPT";

	public static void Render(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle cameraDepth, in TextureHandle motionVectors)
	{
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(cameraData.cameraTargetDescriptor, cameraData.pixelWidth, cameraData.pixelHeight, cameraData.cameraTargetDescriptor.graphicsFormat);
		compatibleDescriptor.enableRandomWrite = true;
		compatibleDescriptor.sRGB = false;
		TextureHandle inputColor = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle destination = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_UpscaledColorTargetSPT", clear: false, FilterMode.Bilinear);
		int frameCount = Time.frameCount;
		Texture2D noiseTexture = processor.Resources.Textures.BlueNoise16Textures[frameCount & (processor.Resources.Textures.BlueNoise16Textures.Length - 1)];
		TextureHandle debugView = TextureHandle.nullHandle;
		int debugViewIndex = 0;
		StpUtils.PopulateStpConfig(cameraData, in inputColor, in cameraDepth, in motionVectors, debugViewIndex, in debugView, in destination, noiseTexture, out var config);
		STP.Execute(renderGraph, ref config);
		PostProcessor.UpdateCameraResolution(processor, renderGraph, cameraData, new Vector2Int(compatibleDescriptor.width, compatibleDescriptor.height));
		processor.CameraStackTargets.SetCurrentPostProcessSource(destination);
	}
}
