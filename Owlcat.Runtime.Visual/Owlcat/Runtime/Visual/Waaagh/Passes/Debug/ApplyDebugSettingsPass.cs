using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class ApplyDebugSettingsPass : ScriptableRenderPass<ApplyDebugSettingsPassData>
{
	private DebugHandler.DebugMipMapTexture m_MipMapTexture;

	public override string Name => "ApplyDebugSettingsPass";

	internal ApplyDebugSettingsPass(RenderPassEvent evt, DebugHandler.DebugMipMapTexture mipMapTexture)
		: base(evt)
	{
		m_MipMapTexture = mipMapTexture;
	}

	protected override void Setup(RenderGraphBuilder builder, ApplyDebugSettingsPassData data, ContextContainer frameData)
	{
		WaaaghDebugData debugData = WaaaghPipeline.Asset.DebugData;
		data.DebugData = debugData;
		data.MipMapTexture = m_MipMapTexture.MipMapTexture;
	}

	protected override void Render(ApplyDebugSettingsPassData data, RenderGraphContext context)
	{
		bool flag = data.DebugData != null && data.DebugData.DebugNeedsDebugDisplayKeyword();
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.DEBUG_DISPLAY, flag);
		if (flag)
		{
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugMaterialMode, (int)data.DebugData.RenderingDebug.DebugMaterialMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugLightingMode, (int)data.DebugData.LightingDebug.DebugLightingMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugOverdrawMode, (int)data.DebugData.RenderingDebug.OverdrawMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugMipMap, data.DebugData.RenderingDebug.DebugMipMap ? 1 : 0);
			if (data.DebugData.RenderingDebug.DebugMipMap)
			{
				Texture.SetStreamingTextureMaterialDebugProperties(0);
			}
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
	}
}
