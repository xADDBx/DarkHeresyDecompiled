using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Virtual Terrain Editor", Order = 1)]
internal sealed class VirtualTerrainEditorGlobalSettings : IRenderPipelineGraphicsSettings
{
	public enum TerrainLayerTextureSizeOptions
	{
		_256 = 0x100,
		_512 = 0x200,
		_1024 = 0x400,
		_2048 = 0x800
	}

	public enum GUIModes
	{
		ShowEverything,
		ShowVirtualTerrainOnly
	}

	[SerializeField]
	[HideInInspector]
	private int m_Version = 3;

	public TerrainLayerTextureSizeOptions TerrainLayerTextureSize = TerrainLayerTextureSizeOptions._1024;

	public int TerrainLayerPvsCellSize = 5;

	public int TerrainLayerPvsExtentRadiusLod0 = 20;

	public int TerrainLayerPvsExtentRadiusLod1 = 40;

	public int EditModeAtlasCapacityLod0 = 32;

	public int EditModeAtlasCapacityLod1 = 128;

	public int EditModeAtlasCapacityLod2 = 256;

	public GUIModes GUIMode;

	public int version => m_Version;

	public bool isAvailableInPlayerBuild => false;
}
