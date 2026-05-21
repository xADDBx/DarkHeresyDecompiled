using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class Distortion
{
	private sealed class DrawOpaqueDepthPassData
	{
		public RendererListHandle DepthRendererList;
	}

	private sealed class DrawOpaqueGBufferPassData
	{
		public RendererListHandle RendererList;
	}

	private sealed class DrawOpaqueColorPassData
	{
		public RendererListHandle ColorRendererList;
	}

	private sealed class DrawDistortionVectorsPassData
	{
		public RendererListHandle DistortionVectorsRendererList;

		public TextureHandle DistortionTexture;

		public TextureHandle CameraColor;

		public TextureHandle CameraDepth;

		public Material ApplyDistortionMaterial;

		public Color ClearColor = new Color(0f, 0f, 0f, 0f);
	}

	private static readonly ShaderTagId s_GBufferLightMode = new ShaderTagId("GBuffer");

	private static readonly ShaderTagId s_DepthOnlyLightMode = new ShaderTagId("DepthOnly");

	private static readonly ShaderTagId[] s_ForwardShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit")
	};

	private static ShaderTagId s_DistortionVectorShaderTag = new ShaderTagId("DistortionVectors");

	public static void DrawOpaqueGBuffer(in RecordContext context)
	{
		DrawOpaqueGBufferPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DrawOpaqueGBufferPassData>("Draw Opaque Distortion GBuffer", out passData, WaaaghProfileId.OpaqueDistortionGBuffer.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Distortion.cs", 50);
		passData.RendererList = CreateOpaqueGBufferRendererList(in context);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseRendererList(in passData.RendererList);
		GBufferUtility.ConfigureDrawPass(unsafeRenderGraphBuilder, in context.FrameResources, context.IsVTEnabled);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DrawOpaqueGBufferPassData data, UnsafeGraphContext context)
		{
			context.cmd.DrawRendererList(data.RendererList);
		});
	}

	public static void DrawOpaqueDepth(in RecordContext context)
	{
		DrawOpaqueDepthPassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<DrawOpaqueDepthPassData>("Draw Opaque Distortion Depth", out passData2, WaaaghProfileId.OpaqueDistortionDepth.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Distortion.cs", 67);
		passData2.DepthRendererList = CreateOpaqueDepthRendererList(in context);
		rasterRenderGraphBuilder.UseRendererList(in passData2.DepthRendererList);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(DrawOpaqueDepthPassData passData, RasterGraphContext context)
		{
			context.cmd.DrawRendererList(passData.DepthRendererList);
		});
	}

	public static void DrawOpaqueColor(in RecordContext context)
	{
		DrawOpaqueColorPassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<DrawOpaqueColorPassData>("Draw Opaque Distortion Color", out passData2, WaaaghProfileId.OpaqueDistortionColor.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Distortion.cs", 82);
		passData2.ColorRendererList = CreateOpaqueColorRendererList(in context);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraColorPyramidRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ShadowmapRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraBakedGIRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraShadowmaskRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId.waaagh_ReflProbes_Atlas);
		rasterRenderGraphBuilder.UseRendererList(in passData2.ColorRendererList);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(DrawOpaqueColorPassData passData, RasterGraphContext context)
		{
			context.cmd.DrawRendererList(passData.ColorRendererList);
		});
	}

	public static void DrawTransparentObjects(in RecordContext context)
	{
		DrawDistortionVectorsPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DrawDistortionVectorsPassData>("Draw Transparent Distortion", out passData2, WaaaghProfileId.TransparentDistortion.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Distortion.cs", 107);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("DistortionRT", context.CameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		passData2.DistortionVectorsRendererList = CreateDistortionVectorsRendererList(in context);
		passData2.DistortionTexture = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData2.CameraColor = context.FrameResources.CameraStackTargets.Color;
		passData2.CameraDepth = context.FrameResources.CameraStackTargets.Depth;
		passData2.ApplyDistortionMaterial = context.MaterialLibrary.ApplyDistortionMaterial;
		unsafeRenderGraphBuilder.UseRendererList(in passData2.DistortionVectorsRendererList);
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraColor);
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraDepth);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraColorPyramidRT);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DrawDistortionVectorsPassData passData, UnsafeGraphContext context)
		{
			context.cmd.SetRenderTarget(passData.DistortionTexture, passData.CameraDepth);
			context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, passData.ClearColor);
			context.cmd.DrawRendererList(passData.DistortionVectorsRendererList);
			context.cmd.SetGlobalTexture(ShaderPropertyId._DistortionVectorsRT, passData.DistortionTexture);
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(passData.DistortionTexture, passData.CameraColor, passData.ApplyDistortionMaterial, 0);
		});
	}

	private static RendererListHandle CreateOpaqueDepthRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_DepthOnlyLightMode, PerObjectData.None, WaaaghRenderQueue.OpaqueDistortion, SortingCriteria.OptimizeStateChanges);
		desc.filteringSettings.batchLayerMask = 5u;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateOpaqueGBufferRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_GBufferLightMode, PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask, WaaaghRenderQueue.OpaqueDistortion, context.CameraData.defaultOpaqueSortFlags);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateOpaqueColorRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_ForwardShaderTags, context.RenderingData.PerObjectData, WaaaghRenderQueue.OpaqueDistortion);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateDistortionVectorsRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_DistortionVectorShaderTag, PerObjectData.None, WaaaghRenderQueue.Transparent);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
