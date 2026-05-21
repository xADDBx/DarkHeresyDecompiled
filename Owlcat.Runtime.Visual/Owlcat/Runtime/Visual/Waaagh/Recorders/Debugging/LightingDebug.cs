using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class LightingDebug
{
	private static class ShaderIDs
	{
		public static readonly int _FeatureTiles = Shader.PropertyToID("_FeatureTiles");

		public static readonly int _ColorStart = Shader.PropertyToID("_ColorStart");

		public static readonly int _ColorEnd = Shader.PropertyToID("_ColorEnd");
	}

	private class ClustersPassData
	{
		public Material Material;

		public int Pass;

		public TextureHandle DepthTexture;

		public BufferHandle FeatureTilesBuffer;
	}

	private class LightSortingCurvePassData
	{
		public Material Material;

		public int TotalLightsCount;

		public Color ColorStart;

		public Color ColorEnd;
	}

	public static void DrawClustersDebug(in RecordContext context)
	{
		DebugContext debugContext = context.DebugContext;
		switch (debugContext.DebugData.LightingDebug.DebugClustersMode)
		{
		case DebugClustersMode.Heatmap:
			Heatmap(debugContext, in context);
			break;
		case DebugClustersMode.HeatmapShadowedLights:
			ShadowedHeatmap(debugContext, context);
			break;
		case DebugClustersMode.DeferredLightingComplexity:
			DeferredLightingComplexity(debugContext, context);
			break;
		case DebugClustersMode.DeferredFeatureVariants:
			FeatureTilesVariants(debugContext, context);
			break;
		case DebugClustersMode.DeferredFeatureTileCoherency:
			TileCoherency(debugContext, context);
			break;
		case DebugClustersMode.None:
			break;
		}
	}

	public static void Heatmap(DebugContext handler, in RecordContext context)
	{
		ClustersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ClustersPassData>("DEBUG - Clusters Heatmap", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 57);
		passData.Material = handler.MaterialLibrary.FullscreenDebug;
		passData.Pass = handler.MaterialLibrary.ClustersHeatmapPass;
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ClustersPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Triangles, 3);
		});
	}

	private static void ShadowedHeatmap(DebugContext handler, RecordContext context)
	{
		ClustersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ClustersPassData>("DEBUG - Clusters Shadowed Heatmap", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 72);
		passData.Material = handler.MaterialLibrary.FullscreenDebug;
		passData.Pass = handler.MaterialLibrary.ClustersShadowedHeatmapPass;
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ClustersPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Triangles, 3);
		});
	}

	private static void DeferredLightingComplexity(DebugContext handler, RecordContext context)
	{
		ClustersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ClustersPassData>("DEBUG - Deferred Lighting Complexity Heatmap", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 87);
		passData.Material = handler.MaterialLibrary.FullscreenDebug;
		passData.Pass = handler.MaterialLibrary.ClustersDeferredLightingComplexityPass;
		passData.DepthTexture = context.FrameResources.CameraStackTargets.Depth;
		rasterRenderGraphBuilder.UseTexture(in passData.DepthTexture);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ClustersPassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.DepthTexture);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Triangles, 3);
		});
	}

	private static void FeatureTilesVariants(DebugContext handler, RecordContext context)
	{
		ClustersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ClustersPassData>("DEBUG - Tile Feature Variants", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 106);
		passData.Material = handler.MaterialLibrary.FullscreenDebug;
		passData.Pass = handler.MaterialLibrary.ClustersFeatureVariantsPass;
		passData.FeatureTilesBuffer = context.FrameResources.DeferredLightingResources.FeatureTilesBuffer;
		rasterRenderGraphBuilder.UseBuffer(in passData.FeatureTilesBuffer);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ClustersPassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalBuffer(ShaderIDs._FeatureTiles, data.FeatureTilesBuffer);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Triangles, 3);
		});
	}

	private static void TileCoherency(DebugContext handler, RecordContext context)
	{
		ClustersPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ClustersPassData>("DEBUG - Tile Coherency", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 125);
		passData.Material = handler.MaterialLibrary.FullscreenDebug;
		passData.Pass = handler.MaterialLibrary.ClustersTileCoherencyPass;
		passData.FeatureTilesBuffer = context.FrameResources.DeferredLightingResources.FeatureTilesBuffer;
		rasterRenderGraphBuilder.UseBuffer(in passData.FeatureTilesBuffer);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ClustersPassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalBuffer(ShaderIDs._FeatureTiles, data.FeatureTilesBuffer);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Pass, MeshTopology.Triangles, 3);
		});
	}

	public static void ShowLightSortingCurve(in RecordContext context)
	{
		DebugContext debugContext = context.DebugContext;
		if (!debugContext.DebugData.LightingDebug.ShowLightSortingCurve)
		{
			return;
		}
		LightSortingCurvePassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<LightSortingCurvePassData>("DEBUG - Light Sorting Curve", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\LightingDebug.cs", 160);
		passData.Material = debugContext.MaterialLibrary.LightSortingCurveMaterial;
		passData.TotalLightsCount = (int)context.Lights.LightDataParams.z;
		passData.ColorStart = debugContext.DebugData.LightingDebug.LightSortingCurveColorStart;
		passData.ColorEnd = debugContext.DebugData.LightingDebug.LightSortingCurveColorEnd;
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(LightSortingCurvePassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalColor(ShaderIDs._ColorStart, data.ColorStart);
			context.cmd.SetGlobalColor(ShaderIDs._ColorEnd, data.ColorEnd);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Lines, 2, data.TotalLightsCount - 1);
		});
	}
}
