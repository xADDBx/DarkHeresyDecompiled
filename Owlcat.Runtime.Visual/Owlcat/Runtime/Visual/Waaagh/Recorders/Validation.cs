using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class Validation
{
	private sealed class PassData
	{
		public RendererListHandle UnsupportedMaterialRendererList;

		public RendererListHandle MissingMaterialRendererList;
	}

	private static readonly ShaderTagId[] s_UnsupportedMaterialPassNames = new ShaderTagId[7]
	{
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("Deferred"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	private static readonly ShaderTagId[] s_MissingMaterialPassNames = new ShaderTagId[1]
	{
		new ShaderTagId("SRPDefaultUnlit")
	};

	private static readonly RenderQueueRange s_UnsupportedMaterialRenderQueueRange = RenderQueueRange.all;

	private static readonly RenderQueueRange s_MissingMaterialRenderQueueRange = RenderQueueRange.opaque;

	public static void DrawObjectWithInvalidMaterials(in RecordContext context)
	{
		PassData passData2;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<PassData>("Draw ErrorObjects", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ForwardPath\\Validation.cs", 36);
		PassData passData3 = passData2;
		RenderGraph renderGraph = context.RenderGraph;
		CullingResults cullResults = context.RenderingData.CullResults;
		Camera camera = context.CameraData.camera;
		ShaderTagId[] passNames = s_UnsupportedMaterialPassNames;
		RenderQueueRange? renderQueueRange = s_UnsupportedMaterialRenderQueueRange;
		Material errorMaterial = context.MaterialLibrary.ErrorMaterial;
		RendererListParams desc = RenderingUtils.CreateRendererListParams(cullResults, camera, passNames, PerObjectData.None, renderQueueRange, SortingCriteria.CommonOpaque, null, errorMaterial);
		passData3.UnsupportedMaterialRendererList = renderGraph.CreateRendererList(in desc);
		PassData passData4 = passData2;
		RenderGraph renderGraph2 = context.RenderGraph;
		CullingResults cullResults2 = context.RenderingData.CullResults;
		Camera camera2 = context.CameraData.camera;
		ShaderTagId[] passNames2 = s_MissingMaterialPassNames;
		RenderQueueRange? renderQueueRange2 = s_MissingMaterialRenderQueueRange;
		errorMaterial = context.MaterialLibrary.ErrorMaterial;
		desc = RenderingUtils.CreateRendererListParams(cullResults2, camera2, passNames2, PerObjectData.None, renderQueueRange2, SortingCriteria.CommonOpaque, null, errorMaterial);
		passData4.MissingMaterialRendererList = renderGraph2.CreateRendererList(in desc);
		rasterRenderGraphBuilder.UseRendererList(in passData2.UnsupportedMaterialRendererList);
		rasterRenderGraphBuilder.UseRendererList(in passData2.MissingMaterialRendererList);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, RasterGraphContext context)
		{
			context.cmd.DrawRendererList(passData.UnsupportedMaterialRendererList);
			context.cmd.DrawRendererList(passData.MissingMaterialRendererList);
		});
	}
}
