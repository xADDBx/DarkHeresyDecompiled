using System.Diagnostics;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class DebugRecorder
{
	private class ApplyDebugSettingsPassData
	{
		public WaaaghDebugData DebugData;

		public Texture2D MipMapTexture;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void ApplyDebugSettings(RenderGraph renderGraph, DebugContext handler)
	{
		if (handler == null)
		{
			return;
		}
		ApplyDebugSettingsPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<ApplyDebugSettingsPassData>("DEBUG - Apply Settings", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\DebugRecorder.cs", 30);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.DebugData = handler.DebugData;
		passData.MipMapTexture = handler.DebugMipMapTexture.MipMapTexture;
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(ApplyDebugSettingsPassData data, UnsafeGraphContext context)
		{
			bool flag = data.DebugData != null && data.DebugData.DebugNeedsDebugDisplayKeyword();
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.DEBUG_DISPLAY, flag);
			if (flag)
			{
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugMaterialMode, (int)data.DebugData.RenderingDebug.DebugMaterialMode);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugLightingMode, (int)data.DebugData.LightingDebug.DebugLightingMode);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugOverdrawMode, (int)data.DebugData.RenderingDebug.OverdrawMode);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugMipMap, data.DebugData.RenderingDebug.DebugMipMap ? 1 : 0);
				context.cmd.SetGlobalTexture(ShaderPropertyId._MipMapDebugMap, data.MipMapTexture);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugVTTiles, (data.DebugData.VirtualTextureDebug.DebugTilesMode != 0) ? 1 : 0);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugVTTilesSliceIndex, (data.DebugData.VirtualTextureDebug.DebugTilesMode == DebugTilesMode.SliceIndex) ? 1 : 0);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugTerrainMode, (int)data.DebugData.TerrainDebug.DebugMode);
				context.cmd.SetGlobalInt(ShaderPropertyId._DebugTerrainLayerIndex, data.DebugData.TerrainDebug.LayerIndex);
				context.cmd.SetGlobalFloat(ShaderPropertyId._DebugTerrainLayerWeightMin, data.DebugData.TerrainDebug.LayerWeightMin);
				context.cmd.SetGlobalFloat(ShaderPropertyId._DebugTerrainLayerWeightMax, data.DebugData.TerrainDebug.LayerWeightMax);
				context.cmd.SetGlobalColor(ShaderPropertyId._DebugTerrainGradientMinColor, data.DebugData.TerrainDebug.GradientMinColor);
				context.cmd.SetGlobalColor(ShaderPropertyId._DebugTerrainGradientMaxColor, data.DebugData.TerrainDebug.GradientMaxColor);
				context.cmd.SetGlobalColor(ShaderPropertyId._DebugTerrainBackColor, data.DebugData.TerrainDebug.BackColor);
				context.cmd.SetGlobalFloat(ShaderPropertyId._DebugTerrainOpacity, data.DebugData.TerrainDebug.Opacity);
				context.cmd.SetGlobalFloat(ShaderPropertyId._DebugTerrainPvsHeatmapCountThreshold, data.DebugData.TerrainDebug.PvsHeatmapCountThreshold);
				Vector3 vector = data.DebugData.TerrainDebug.ResolveSplatMapMaskPosition();
				float num = data.DebugData.TerrainDebug.ResolveSplatMapMaskRadius();
				context.cmd.SetGlobalVector(ShaderPropertyId._DebugTerrainSplatMapWeightMaskShape, new Vector4(vector.x, vector.z, num * num));
			}
		});
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void Prepare(in RecordContext context)
	{
		GpuDrivenDebug.Prepare(in context);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void DrawDebugOverlay(in RecordContext context)
	{
		if (context.DebugContext != null)
		{
			LightingDebug.DrawClustersDebug(in context);
			LightingDebug.ShowLightSortingCurve(in context);
			MapOverlays.Record(in context);
			StencilDebug.Record(in context);
			ShadowsDebug.Record(in context);
			VirtualTextureDebug.Record(in context);
			GpuDrivenDebug.DrawOverlayAndFinish(in context);
		}
	}
}
