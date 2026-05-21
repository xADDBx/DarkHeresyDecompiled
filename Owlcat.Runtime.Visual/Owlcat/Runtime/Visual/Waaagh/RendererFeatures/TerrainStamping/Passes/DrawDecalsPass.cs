using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.Waaagh.Recorders;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

internal static class DrawDecalsPass
{
	private static class Profiling
	{
		public static readonly ProfilingSampler TerrainStampingDrawDecals = new ProfilingSampler("TerrainStampingDrawDecals");

		public static readonly ProfilingSampler FillMask = new ProfilingSampler("FillMask");

		public static readonly ProfilingSampler ClearMask = new ProfilingSampler("ClearMask");

		public static readonly ProfilingSampler StampingDecals = new ProfilingSampler("StampingDecals");
	}

	private sealed class PassData
	{
		public Material StencilMaskMaterial;

		public bool TransitionBlendDitheringEnabled;

		public RendererListHandle DecalRendererList;
	}

	private static readonly ShaderTagId s_TerrainStampingDecalDeferredShaderTag = new ShaderTagId("TerrainStampingDecalDeferred");

	public static void Record(in RecordContext context, TerrainStampingManagerParameters parameters, Material stencilMaskMaterial)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Terrain Stamping Draw Decals", out passData2, Profiling.TerrainStampingDrawDecals, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\TerrainStamping\\Passes\\DrawDecalsPass.cs", 35);
		passData2.StencilMaskMaterial = stencilMaskMaterial;
		passData2.TransitionBlendDitheringEnabled = OwlcatTerrainTransition.Active && parameters.TransitionBlendDithering;
		passData2.DecalRendererList = CreateDecalRendererList(in context);
		Decals.SetupDeferredDecalsDrawPass(in context, unsafeRenderGraphBuilder);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.DecalRendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			CoreUtils.SetKeyword(context.cmd, "_TERRAIN_TRANSITION_BLEND_DITHERING", passData.TransitionBlendDitheringEnabled);
			using (new ProfilingScope(context.cmd, Profiling.FillMask))
			{
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.StencilMaskMaterial, 0, MeshTopology.Triangles, 3);
			}
			using (new ProfilingScope(context.cmd, Profiling.StampingDecals))
			{
				context.cmd.DrawRendererList(passData.DecalRendererList);
			}
			using (new ProfilingScope(context.cmd, Profiling.ClearMask))
			{
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.StencilMaskMaterial, 1, MeshTopology.Triangles, 3);
			}
		});
	}

	private static RendererListHandle CreateDecalRendererList(in RecordContext context)
	{
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		filteringSettings.batchLayerMask = 4294967283u;
		FilteringSettings filteringSettings2 = filteringSettings;
		SortingSettings sortingSettings = new SortingSettings(context.CameraData.camera);
		sortingSettings.criteria = SortingCriteria.RenderQueue;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(s_TerrainStampingDecalDeferredShaderTag, sortingSettings2);
		drawingSettings.perObjectData = context.RenderingData.PerObjectData;
		drawingSettings.enableInstancing = true;
		drawingSettings.enableDynamicBatching = false;
		DrawingSettings drawSettings = drawingSettings;
		RendererListParams desc = new RendererListParams(context.RenderingData.CullResults, drawSettings, filteringSettings2);
		return context.RenderGraph.CreateRendererList(in desc);
	}
}
