using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class Overlay
{
	private sealed class PassData
	{
		public RendererListHandle OverlayObjectsRendererList;
	}

	private static readonly ShaderTagId[] s_OverlayShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit")
	};

	public static void DrawObjects(in RecordContext context)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Overlay", out passData2, WaaaghProfileId.DrawObjects_Overlay.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Overlay.cs", 22);
		passData2.OverlayObjectsRendererList = CreateOverlayObjectsRendererList(in context);
		if (context.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			BufferHandle input = context.GPUDrivenBatchRendererGroup.SharedPassData.Buffers.ForwardReflectionProbeIndices;
			if (input.IsValid())
			{
				unsafeRenderGraphBuilder.UseBuffer(in input);
			}
		}
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._ShadowmapRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId.waaagh_ReflProbes_Atlas);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.OverlayObjectsRendererList);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			context.cmd.DrawRendererList(passData.OverlayObjectsRendererList);
		});
	}

	private static RendererListHandle CreateOverlayObjectsRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_OverlayShaderTags, -1, context.RenderingData.PerObjectData, WaaaghRenderQueue.Overlay, SortingCriteria.CommonTransparent);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
