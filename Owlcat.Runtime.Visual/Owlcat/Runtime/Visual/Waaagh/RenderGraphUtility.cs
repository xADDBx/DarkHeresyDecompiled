using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class RenderGraphUtility
{
	public static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
	{
		TextureDesc desc2 = new TextureDesc(desc.width, desc.height);
		desc2.dimension = desc.dimension;
		desc2.clearBuffer = clear;
		desc2.bindTextureMS = desc.bindMS;
		desc2.colorFormat = desc.graphicsFormat;
		desc2.depthBufferBits = (DepthBits)desc.depthBufferBits;
		desc2.slices = desc.volumeDepth;
		desc2.msaaSamples = (MSAASamples)desc.msaaSamples;
		desc2.name = name;
		desc2.enableRandomWrite = desc.enableRandomWrite;
		desc2.filterMode = filterMode;
		desc2.wrapMode = wrapMode;
		desc2.isShadowMap = desc.shadowSamplingMode != ShadowSamplingMode.None && desc.depthStencilFormat != GraphicsFormat.None;
		desc2.vrUsage = desc.vrUsage;
		return renderGraph.CreateTexture(in desc2);
	}
}
