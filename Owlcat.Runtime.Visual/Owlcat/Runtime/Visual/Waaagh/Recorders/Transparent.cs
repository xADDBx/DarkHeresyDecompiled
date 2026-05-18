using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class Transparent
{
	private sealed class PassData
	{
		public RendererListHandle TransparentObjectsRendererList;
	}

	private static readonly ShaderTagId[] s_TransparentShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit")
	};

	public static void DrawObjects(in RecordContext context)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Transparent", out passData2, WaaaghProfileId.DrawObjects_Transparent.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Transparent.cs", 24);
		passData2.TransparentObjectsRendererList = CreateTransparentObjectsRendererList(in context);
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
		unsafeRenderGraphBuilder.AllowPassCulling(!context.CameraData.IrsData.IrsHasTransparents);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.TransparentObjectsRendererList);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		if (context.IsVTEnabled)
		{
			unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.VTFeedbackData.VTFeedbackMRT, 1);
		}
		unsafeRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			context.cmd.DrawRendererList(passData.TransparentObjectsRendererList);
		});
	}

	private static RendererListHandle CreateTransparentObjectsRendererList(in RecordContext context)
	{
		RendererListParams desc = RenderingUtils.CreateRendererListParams(context.RenderingData.CullResults, context.CameraData.camera, s_TransparentShaderTags, -1, context.RenderingData.PerObjectData, WaaaghRenderQueue.Transparent, SortingCriteria.CommonTransparent);
		desc.filteringSettings.batchLayerMask = 4294967283u;
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
