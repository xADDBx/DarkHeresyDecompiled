using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class DebugMaterialLibrary : IDisposable
{
	private List<Material> m_Materials = new List<Material>();

	public Material FullscreenDebug { get; }

	public int ClustersHeatmapPass { get; }

	public int ClustersShadowedHeatmapPass { get; }

	public int ClustersDeferredLightingComplexityPass { get; }

	public int ClustersFeatureVariantsPass { get; }

	public int ClustersTileCoherencyPass { get; }

	public int StencilDebugPass { get; }

	public int DebugBlitPass { get; }

	public Material ShadowsDebug { get; }

	public Material VirtualTextureDebug { get; }

	public int ShowFeedbackTexturePass { get; }

	public int DecodeFeedbackPass { get; }

	public int PhysicalAtlasDebugPass { get; }

	public int ShowBatchedCopyRtPass { get; }

	public int IndirectTextureDebugPass { get; }

	public int VirtualAtlasDebugPass { get; }

	public Material GpuDrivenDebug { get; }

	public int ShowOcclusionTestPass { get; }

	public Material LightSortingCurveMaterial { get; }

	public Material DebugOverdrawMaterial { get; }

	public int DebugOverdrawPassQuadOverdrawOpaque { get; }

	public int DebugOverdrawPassQuadOverdrawOpaqueIrs { get; }

	public int DebugOverdrawPassQuadOverdrawTransparent { get; }

	public int DebugOverdrawPassQuadOverdrawBlit { get; }

	public int DebugOverdrawPassOverdrawBlit { get; }

	public DebugMaterialLibrary(DebugResources resources)
	{
		FullscreenDebug = CreateMat(resources.DebugFullscreenPS);
		ClustersHeatmapPass = FullscreenDebug.FindPass("TILES HEATMAP");
		ClustersShadowedHeatmapPass = FullscreenDebug.FindPass("CLUSTERS HEATMAP SHADOWS");
		ClustersDeferredLightingComplexityPass = FullscreenDebug.FindPass("DEFERRED LIGHTING COMPLEXITY");
		ClustersFeatureVariantsPass = FullscreenDebug.FindPass("DEFERRED_FEATURE_VARIANTS");
		ClustersTileCoherencyPass = FullscreenDebug.FindPass("DEFERRED_FEATURE_TILE_COHERENCY");
		StencilDebugPass = FullscreenDebug.FindPass("STENCIL DEBUG");
		DebugBlitPass = FullscreenDebug.FindPass("DEBUG BLIT");
		ShadowsDebug = CreateMat(resources.ShadowsDebugPS);
		VirtualTextureDebug = CreateMat(resources.VirtualTextureDebugPS);
		ShowFeedbackTexturePass = VirtualTextureDebug.FindPass("FeedbackDebug");
		DecodeFeedbackPass = VirtualTextureDebug.FindPass("DecodeFeedback");
		PhysicalAtlasDebugPass = VirtualTextureDebug.FindPass("PhysicalAtlasDebug");
		ShowBatchedCopyRtPass = VirtualTextureDebug.FindPass("ShowBatchedCopyRt");
		IndirectTextureDebugPass = VirtualTextureDebug.FindPass("IndirectTextureDebug");
		VirtualAtlasDebugPass = VirtualTextureDebug.FindPass("VirtualAtlasDebug");
		GpuDrivenDebug = CreateMat(resources.GPUDrivenDebugPS);
		ShowOcclusionTestPass = GpuDrivenDebug.FindPass("ShowOcclusionTest");
		LightSortingCurveMaterial = CreateMat(resources.ShowLightSortingCurvePS);
		DebugOverdrawMaterial = CreateMat(resources.DebugOverdrawPS);
		DebugOverdrawMaterial.enableInstancing = true;
		DebugOverdrawPassQuadOverdrawOpaque = DebugOverdrawMaterial.FindPass("QUAD_OVERDRAW_OPAQUE");
		DebugOverdrawPassQuadOverdrawOpaqueIrs = DebugOverdrawMaterial.FindPass("QUAD_OVERDRAW_OPAQUE_IRS");
		DebugOverdrawPassQuadOverdrawTransparent = DebugOverdrawMaterial.FindPass("QUAD_OVERDRAW_TRANSPARENT");
		DebugOverdrawPassQuadOverdrawBlit = DebugOverdrawMaterial.FindPass("QUAD_OVERDRAW_BLIT");
		DebugOverdrawPassOverdrawBlit = DebugOverdrawMaterial.FindPass("OVERDRAW_BLIT");
	}

	private Material CreateMat(Shader shader)
	{
		Material material = CoreUtils.CreateEngineMaterial(shader);
		m_Materials.Add(material);
		return material;
	}

	public void Dispose()
	{
		foreach (Material material in m_Materials)
		{
			CoreUtils.Destroy(material);
		}
		m_Materials.Clear();
	}
}
