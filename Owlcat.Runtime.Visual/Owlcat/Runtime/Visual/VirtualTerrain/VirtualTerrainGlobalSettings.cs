using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Virtual Terrain", Order = 1)]
internal sealed class VirtualTerrainGlobalSettings : IRenderPipelineGraphicsSettings
{
	public static readonly VirtualTerrainGlobalSettings Default = new VirtualTerrainGlobalSettings
	{
		m_Version = 1,
		Enabled = false,
		TerrainLayerTiling = 0.5f,
		AtlasCapacityLod0 = 8,
		AtlasCapacityLod1 = 16,
		AtlasCapacityLod2 = 128
	};

	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	public bool Enabled;

	public float TerrainLayerTiling = 0.5f;

	public int AtlasCapacityLod0 = 8;

	public int AtlasCapacityLod1 = 16;

	public int AtlasCapacityLod2 = 128;

	public int version => m_Version;

	public bool isAvailableInPlayerBuild => true;
}
