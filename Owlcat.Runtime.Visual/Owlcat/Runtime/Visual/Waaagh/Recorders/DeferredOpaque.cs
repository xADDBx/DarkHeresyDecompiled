using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class DeferredOpaque
{
	private class DepthPrePassData
	{
		public RendererListHandle BaseRendererList;

		public RendererListHandle AlphaRendererList;

		public CameraType CameraType;

		public bool IsIndirectRenderingEnabled;

		public bool IsSceneViewInPrefabEditMode;
	}

	private class GBufferPassData
	{
		public RendererListHandle BaseRendererList;

		public RendererListHandle AlphaRendererList;

		public CameraType CameraType;

		public bool IsIndirectRenderingEnabled;

		public bool IsSceneViewInPrefabEditMode;
	}

	private const WaaaghProfileId DepthOnlyBaseProfileId = WaaaghProfileId.DepthPrePass_OpaqueBase;

	private const WaaaghProfileId DepthOnlyAlphaProfileId = WaaaghProfileId.DepthPrePass_OpaqueAlphaTest;

	private const WaaaghProfileId GBufferBaseProfileId = WaaaghProfileId.GBuffer_OpaqueBase;

	private const WaaaghProfileId GBufferAlphaProfileId = WaaaghProfileId.GBuffer_OpaqueAlphaTest;

	private static readonly ShaderTagId DepthOnlyShaderTag = new ShaderTagId("DepthOnly");

	private static readonly ShaderTagId[] GBufferShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("GBuffer"),
		new ShaderTagId("TerrainGBuffer")
	};

	private static readonly ShaderTagId[] IrsDepthOnlyShaderTags = new ShaderTagId[1]
	{
		new ShaderTagId("DepthOnly")
	};

	private static readonly ShaderTagId[] IrsGBufferShaderTags = new ShaderTagId[1]
	{
		new ShaderTagId("GBuffer")
	};

	private static RenderQueueRange OpaqueBaseQueueRange => WaaaghRenderQueue.Opaque;

	private static RenderQueueRange OpaqueAlphaQueueRange => WaaaghRenderQueue.OpaqueAlphaTest;

	public static void DrawDepthPrePass(in RecordContext context)
	{
		DepthPrePassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DepthPrePassData>("Draw Depth Only", out passData2, WaaaghProfileId.DepthPrePass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredOpaque.cs", 43);
		passData2.BaseRendererList = CreateDepthOnlyRendererList(in context, OpaqueBaseQueueRange);
		passData2.AlphaRendererList = CreateDepthOnlyRendererList(in context, OpaqueAlphaQueueRange);
		passData2.CameraType = context.CameraData.cameraType;
		passData2.IsIndirectRenderingEnabled = context.CameraData.IrsData.Enabled;
		passData2.IsSceneViewInPrefabEditMode = context.CameraData.IsSceneViewInPrefabEditMode;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.BaseRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.AlphaRendererList);
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DepthPrePassData passData, UnsafeGraphContext context)
		{
			using (new ProfilingScope(context.cmd, WaaaghProfileId.DepthPrePass_OpaqueBase.Sampler()))
			{
				context.cmd.DrawRendererList(passData.BaseRendererList);
				IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: false, OpaqueBaseQueueRange, IrsDepthOnlyShaderTags);
			}
			using (new ProfilingScope(context.cmd, WaaaghProfileId.DepthPrePass_OpaqueAlphaTest.Sampler()))
			{
				context.cmd.DrawRendererList(passData.AlphaRendererList);
				IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: false, OpaqueAlphaQueueRange, IrsDepthOnlyShaderTags);
			}
		});
	}

	public static void DrawGBufferPass(in RecordContext context)
	{
		GBufferPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<GBufferPassData>("Draw GBuffer", out passData2, WaaaghProfileId.GBufferPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\DeferredOpaque.cs", 89);
		passData2.BaseRendererList = CreateGBufferRendererList(in context, OpaqueBaseQueueRange);
		passData2.AlphaRendererList = CreateGBufferRendererList(in context, OpaqueAlphaQueueRange);
		passData2.CameraType = context.CameraData.cameraType;
		passData2.IsIndirectRenderingEnabled = context.CameraData.IrsData.Enabled;
		passData2.IsSceneViewInPrefabEditMode = context.CameraData.IsSceneViewInPrefabEditMode;
		GBufferUtility.ConfigureDrawPass(unsafeRenderGraphBuilder, in context.FrameResources, context.IsVTEnabled);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.BaseRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.AlphaRendererList);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Albedo, GlobalTextureShaderPropertyId._CameraAlbedoRT);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Specular, GlobalTextureShaderPropertyId._CameraSpecularRT);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Normals, GlobalTextureShaderPropertyId._CameraNormalsRT);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Normals, GlobalTextureShaderPropertyId._CameraNormalsTexture);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Translucency, GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.BakedGI, GlobalTextureShaderPropertyId._CameraBakedGIRT);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in context.FrameResources.GBuffer.Shadowmask, GlobalTextureShaderPropertyId._CameraShadowmaskRT);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(GBufferPassData passData, UnsafeGraphContext context)
		{
			using (new ProfilingScope(context.cmd, WaaaghProfileId.GBuffer_OpaqueBase.Sampler()))
			{
				context.cmd.DrawRendererList(passData.BaseRendererList);
				IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: false, OpaqueBaseQueueRange, IrsGBufferShaderTags);
			}
			using (new ProfilingScope(context.cmd, WaaaghProfileId.GBuffer_OpaqueAlphaTest.Sampler()))
			{
				context.cmd.DrawRendererList(passData.AlphaRendererList);
				IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: false, OpaqueAlphaQueueRange, IrsGBufferShaderTags);
			}
		});
	}

	private static RendererListHandle CreateGBufferRendererList(in RecordContext context, RenderQueueRange renderQueueRange)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, GBufferShaderTags, PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask, renderQueueRange, SortingCriteria.None);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateDepthOnlyRendererList(in RecordContext context, RenderQueueRange renderQueueRange)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, DepthOnlyShaderTag, PerObjectData.None, renderQueueRange, SortingCriteria.OptimizeStateChanges);
		desc.filteringSettings.batchLayerMask = 5u;
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
