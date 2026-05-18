using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class Decals
{
	private sealed class BuildDBufferPassData
	{
		public Material DBufferBlitMaterial;

		public int DBufferBlitMaterialUnpackPass;
	}

	private sealed class ResolveDBufferPassData
	{
		public Material DBufferBlitMaterial;

		public int DBufferBlitMaterialPackPass;
	}

	private sealed class DrawDeferredDecalsPassData
	{
		public RendererListHandle DecalsRendererList;
	}

	private sealed class DrawForwardDecalsPassData
	{
		public RendererListHandle DecalsRendererList;
	}

	private static readonly ShaderTagId[] DeferredDecalsLightModeTags = new ShaderTagId[1]
	{
		new ShaderTagId("DecalDeferred")
	};

	private static readonly ShaderTagId[] ForwardDecalsLightModeTags = new ShaderTagId[2]
	{
		new ShaderTagId("DecalGUI"),
		new ShaderTagId("DecalForwardOverlay")
	};

	public static readonly ProfilingSampler DeferredDecalsProfilingSampler = new ProfilingSampler("Deferred Decals");

	public static void InitializeDBuffer(in RecordContext context)
	{
		BuildDBufferPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<BuildDBufferPassData>("Initialize DBuffer", out passData, WaaaghProfileId.InitializeDBuffer.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Decals.cs", 49);
		passData.DBufferBlitMaterial = context.MaterialLibrary.DecalBufferBlitMaterial;
		passData.DBufferBlitMaterialUnpackPass = context.MaterialLibrary.DecalBufferBlitMaterialUnpackPass;
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.DBuffer.Masks, 0, AccessFlags.WriteAll);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.DBuffer.Normals, 1, AccessFlags.WriteAll);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.DBuffer.Masks, GlobalTextureShaderPropertyId._DecalsMasksRT);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.DBuffer.Normals, GlobalTextureShaderPropertyId._DecalsNormalsRT);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(BuildDBufferPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, data.DBufferBlitMaterialUnpackPass, MeshTopology.Triangles, 3);
		});
	}

	public static void ResolveDBuffer(in RecordContext context)
	{
		ResolveDBufferPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ResolveDBufferPassData>("Resolve DBuffer", out passData, WaaaghProfileId.ResolveDBuffer.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Decals.cs", 71);
		passData.DBufferBlitMaterial = context.MaterialLibrary.DecalBufferBlitMaterial;
		passData.DBufferBlitMaterialPackPass = context.MaterialLibrary.DecalBufferBlitMaterialPackPass;
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraAlbedoRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._DecalsMasksRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._DecalsNormalsRT);
		rasterRenderGraphBuilder.AllowPassCulling(value: false);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.GBuffer.Normals, 0);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.GBuffer.Specular, 1);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.GBuffer.Translucency, 2);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ResolveDBufferPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, data.DBufferBlitMaterialPackPass, MeshTopology.Triangles, 3);
		});
	}

	public static void SetupDeferredDecalsDrawPass(in RecordContext context, IRenderAttachmentRenderGraphBuilder builder)
	{
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraSpecularRT);
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraBakedGIRT);
		builder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraShadowmaskRT);
		builder.SetRenderAttachment(context.FrameResources.GBuffer.Albedo, 0);
		builder.SetRenderAttachment(context.FrameResources.DBuffer.Masks, 1);
		builder.SetRenderAttachment(context.FrameResources.DBuffer.Normals, 2);
		builder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 3);
		if (context.IsVTEnabled)
		{
			builder.SetRenderAttachment(context.FrameResources.VTFeedbackData.VTFeedbackMRT, 4);
		}
		builder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
	}

	public static void DrawDeferredDecals(in RecordContext context)
	{
		DrawDeferredDecalsPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DrawDeferredDecalsPassData>("Draw Deferred Decals", out passData, WaaaghProfileId.DrawDeferredDecals.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Decals.cs", 116);
		passData.DecalsRendererList = CreateDeferredDecalsRendererList(in context);
		SetupDeferredDecalsDrawPass(in context, unsafeRenderGraphBuilder);
		unsafeRenderGraphBuilder.UseRendererList(in passData.DecalsRendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DrawDeferredDecalsPassData data, UnsafeGraphContext context)
		{
			context.cmd.DrawRendererList(data.DecalsRendererList);
		});
	}

	public static void DrawForwardDecals(in RecordContext context)
	{
		DrawForwardDecalsPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DrawForwardDecalsPassData>("Draw Forward Decals", out passData, WaaaghProfileId.DrawForwardDecals.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Decals.cs", 131);
		passData.DecalsRendererList = CreateForwardDecalsRendererList(in context);
		unsafeRenderGraphBuilder.UseRendererList(in passData.DecalsRendererList);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraSpecularRT);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DrawForwardDecalsPassData data, UnsafeGraphContext context)
		{
			context.cmd.DrawRendererList(data.DecalsRendererList);
		});
	}

	public static TextureDesc GetDBufferMasksTextureDesc(in WaaaghCameraData cameraData)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc("DBuffer0_Masks", cameraData.cameraTargetDescriptor);
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		result.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		return result;
	}

	public static TextureDesc GetDBufferNormalTextureDesc(in WaaaghCameraData cameraData)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc("DBuffer1_Normals", cameraData.cameraTargetDescriptor);
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		result.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		return result;
	}

	private static RendererListHandle CreateDeferredDecalsRendererList(in RecordContext context)
	{
		return CreateDecalsRendererList(in context, DeferredDecalsLightModeTags);
	}

	private static RendererListHandle CreateForwardDecalsRendererList(in RecordContext context)
	{
		return CreateDecalsRendererList(in context, ForwardDecalsLightModeTags);
	}

	private static RendererListHandle CreateDecalsRendererList(in RecordContext context, ShaderTagId[] lightModeShaderTags)
	{
		SortingSettings sortingSettings = new SortingSettings(context.CameraData.camera);
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(lightModeShaderTags[0], sortingSettings2);
		drawingSettings.perObjectData = context.RenderingData.PerObjectData;
		drawingSettings.enableInstancing = true;
		drawingSettings.enableDynamicBatching = false;
		DrawingSettings drawSettings = drawingSettings;
		for (int i = 1; i < lightModeShaderTags.Length; i++)
		{
			drawSettings.SetShaderPassName(i, lightModeShaderTags[i]);
		}
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		filteringSettings.batchLayerMask = 4294967283u;
		FilteringSettings filteringSettings2 = filteringSettings;
		RendererListParams desc = new RendererListParams(context.RenderingData.CullResults, drawSettings, filteringSettings2);
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
