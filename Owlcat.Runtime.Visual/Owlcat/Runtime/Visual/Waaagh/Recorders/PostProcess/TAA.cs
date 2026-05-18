using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class TAA
{
	private const string _TemporalAATargetName = "_TemporalAATarget";

	public static void Render(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle cameraDepth, in TextureHandle motionVectors)
	{
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle srcColor = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle dstColor = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_TemporalAATarget", clear: false, FilterMode.Bilinear);
		TemporalAA.Render(renderGraph, processor.MatLib.TemporalAntialiasing, cameraData, in srcColor, in cameraDepth, in motionVectors, ref dstColor);
		processor.CameraStackTargets.SetCurrentPostProcessSource(dstColor);
	}

	private static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
	{
		desc.depthBufferBits = (int)depthBufferBits;
		desc.msaaSamples = 1;
		desc.width = width;
		desc.height = height;
		desc.graphicsFormat = format;
		return desc;
	}
}
