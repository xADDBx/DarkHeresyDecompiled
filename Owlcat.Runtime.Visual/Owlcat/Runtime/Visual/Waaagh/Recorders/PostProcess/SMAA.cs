using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class SMAA
{
	private class SMAASetupPassData
	{
		internal Vector4 metrics;

		internal Texture2D areaTexture;

		internal Texture2D searchTexture;

		internal float stencilRef;

		internal float stencilMask;

		internal AntialiasingQuality antialiasingQuality;

		internal Material material;
	}

	private class SMAAPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle depthStencilTexture;

		internal TextureHandle blendTexture;

		internal Material material;
	}

	public static void Render(PostProcessor postProcessor, RenderGraph renderGraph, AntialiasingQuality quality, in TextureHandle cameraDepthBuffer)
	{
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		GraphicsFormat sMAAEdgeFormat = postProcessor.StaticState.SMAAEdgeFormat;
		TextureHandle input = postProcessor.CameraStackTargets.CurrentPostProcessSource;
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle textureHandle = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_SMAATarget", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor2 = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, sMAAEdgeFormat);
		TextureHandle input2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor2, "_EdgeStencilTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor3 = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, GraphicsFormat.None, DepthBits.Depth24);
		TextureHandle textureHandle2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor3, "_EdgeTexture", clear: true, FilterMode.Bilinear);
		RenderTextureDescriptor compatibleDescriptor4 = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, GraphicsFormat.R8G8B8A8_UNorm);
		TextureHandle input3 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor4, "_BlendTexture", clear: true);
		Material sMAA = postProcessor.MatLib.SMAA;
		SMAASetupPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<SMAASetupPassData>("SMAA Material Setup", out passData, WaaaghProfileId.SMAAMaterialSetup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\SMAA.cs", 67))
		{
			passData.metrics = new Vector4(1f / (float)descriptor.width, 1f / (float)descriptor.height, descriptor.width, descriptor.height);
			passData.areaTexture = postProcessor.Resources.Textures.SmaaAreaTex;
			passData.searchTexture = postProcessor.Resources.Textures.SmaaSearchTex;
			passData.stencilRef = 64f;
			passData.stencilMask = 64f;
			passData.antialiasingQuality = quality;
			passData.material = sMAA;
			rasterRenderGraphBuilder.AllowPassCulling(value: false);
			rasterRenderGraphBuilder.SetRenderFunc(delegate(SMAASetupPassData data, RasterGraphContext context)
			{
				data.material.SetVector(PostProcessor.ShaderIDs._Metrics, data.metrics);
				data.material.SetTexture(PostProcessor.ShaderIDs._AreaTexture, data.areaTexture);
				data.material.SetTexture(PostProcessor.ShaderIDs._SearchTexture, data.searchTexture);
				data.material.SetFloat(PostProcessor.ShaderIDs._StencilRef, data.stencilRef);
				data.material.SetFloat(PostProcessor.ShaderIDs._StencilMask, data.stencilMask);
				data.material.shaderKeywords = null;
				switch (data.antialiasingQuality)
				{
				case AntialiasingQuality.Low:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaLow);
					break;
				case AntialiasingQuality.Medium:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaMedium);
					break;
				case AntialiasingQuality.High:
					data.material.EnableKeyword(ShaderKeywordStrings.SmaaHigh);
					break;
				}
			});
		}
		SMAAPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Edge Detection", out passData2, WaaaghProfileId.SMAAEdgeDetection.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\SMAA.cs", 108))
		{
			passData2.destinationTexture = input2;
			rasterRenderGraphBuilder2.SetRenderAttachment(input2, 0);
			passData2.depthStencilTexture = textureHandle2;
			rasterRenderGraphBuilder2.SetRenderAttachmentDepth(textureHandle2);
			passData2.sourceTexture = input;
			rasterRenderGraphBuilder2.UseTexture(in input);
			rasterRenderGraphBuilder2.UseTexture(in cameraDepthBuffer);
			passData2.material = sMAA;
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material3 = data.material;
				RasterCommandBuffer cmd3 = context.cmd;
				RTHandle rTHandle3 = data.sourceTexture;
				Vector2 vector3 = (rTHandle3.useScaling ? new Vector2(rTHandle3.rtHandleProperties.rtHandleScale.x, rTHandle3.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd3, rTHandle3, vector3, material3, 0);
			});
		}
		SMAAPassData passData3;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Blend weights", out passData3, WaaaghProfileId.SMAABlendWeight.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\SMAA.cs", 131))
		{
			passData3.destinationTexture = input3;
			rasterRenderGraphBuilder3.SetRenderAttachment(input3, 0);
			passData3.depthStencilTexture = textureHandle2;
			rasterRenderGraphBuilder3.SetRenderAttachmentDepth(textureHandle2, AccessFlags.Read);
			passData3.sourceTexture = input2;
			rasterRenderGraphBuilder3.UseTexture(in input2);
			passData3.material = sMAA;
			rasterRenderGraphBuilder3.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material2 = data.material;
				RasterCommandBuffer cmd2 = context.cmd;
				RTHandle rTHandle2 = data.sourceTexture;
				Vector2 vector2 = (rTHandle2.useScaling ? new Vector2(rTHandle2.rtHandleProperties.rtHandleScale.x, rTHandle2.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd2, rTHandle2, vector2, material2, 1);
			});
		}
		SMAAPassData passData4;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder4 = renderGraph.AddRasterRenderPass<SMAAPassData>("SMAA Neighborhood blending", out passData4, WaaaghProfileId.SMAANeighborhoodBlend.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\SMAA.cs", 153))
		{
			rasterRenderGraphBuilder4.AllowGlobalStateModification(value: true);
			passData4.destinationTexture = textureHandle;
			rasterRenderGraphBuilder4.SetRenderAttachment(textureHandle, 0);
			passData4.sourceTexture = input;
			rasterRenderGraphBuilder4.UseTexture(in input);
			passData4.blendTexture = input3;
			rasterRenderGraphBuilder4.UseTexture(in input3);
			passData4.material = sMAA;
			rasterRenderGraphBuilder4.SetRenderFunc(delegate(SMAAPassData data, RasterGraphContext context)
			{
				Material material = data.material;
				RasterCommandBuffer cmd = context.cmd;
				RTHandle rTHandle = data.sourceTexture;
				material.SetTexture(PostProcessor.ShaderIDs._BlendTexture, data.blendTexture);
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd, rTHandle, vector, material, 2);
			});
		}
		postProcessor.CameraStackTargets.SetCurrentPostProcessSource(textureHandle);
	}
}
