using System.Collections.Generic;
using Owlcat.Runtime.Visual.VirtualTerrain;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

internal sealed class DebugTerrainOverlay
{
	private struct Heatmap
	{
		public OwlcatVirtualTerrain VirtualTerrain;

		public Texture2D Texture;
	}

	private readonly WaaaghDebugData m_Data;

	private readonly List<Heatmap> m_Heatmaps = new List<Heatmap>();

	private bool m_HeatmapsCreated;

	public DebugTerrainOverlay(WaaaghDebugData data)
	{
		m_Data = data;
	}

	public void Setup()
	{
		if (m_Data.TerrainDebug != null && m_Data.TerrainDebug.DebugMode == TerrainDebugMode.LayerBudgetOverrun)
		{
			if (m_HeatmapsCreated && m_Data.TerrainDebug.HeatmapRefreshRequested)
			{
				m_Data.TerrainDebug.HeatmapRefreshRequested = false;
				m_HeatmapsCreated = false;
				DestroyHeatmaps();
			}
			if (!m_HeatmapsCreated)
			{
				m_HeatmapsCreated = true;
				CreateHeatmaps();
			}
		}
		else if (m_HeatmapsCreated)
		{
			m_HeatmapsCreated = false;
			DestroyHeatmaps();
		}
	}

	public void Cleanup()
	{
		DestroyHeatmaps();
	}

	private void CreateHeatmaps()
	{
	}

	private void DestroyHeatmaps()
	{
	}

	private static Texture2D CreatePvsHeatmap(UnityEngine.Terrain terrain)
	{
		if (!GraphicsSettings.TryGetRenderPipelineSettings<VirtualTerrainEditorGlobalSettings>(out var settings))
		{
			return Texture2D.blackTexture;
		}
		TerrainLayerPVS terrainLayerPVS = TerrainLayerPVSFactory.Create(terrain, settings.TerrainLayerPvsCellSize, settings.TerrainLayerPvsExtentRadiusLod0, settings.TerrainLayerPvsExtentRadiusLod1);
		Texture2D texture2D = new Texture2D(terrainLayerPVS.NodesCount.x, terrainLayerPVS.NodesCount.y, GraphicsFormat.R8_UNorm, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate);
		texture2D.hideFlags |= HideFlags.DontSave;
		texture2D.name = "DebugTerrainHeatMap_" + terrain.name;
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		NativeArray<byte> rawTextureData = texture2D.GetRawTextureData<byte>();
		for (int i = 0; i < rawTextureData.Length; i++)
		{
			rawTextureData[i] = (byte)math.countbits(terrainLayerPVS.Nodes0[i]);
		}
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		return texture2D;
	}
}
