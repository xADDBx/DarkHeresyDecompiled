using System.Collections.Generic;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class TerrainLayerId
{
	private static readonly Dictionary<TerrainLayerGuid, int> s_TerrainLayerGuidToIdMap;

	private static readonly List<TerrainLayerGuid> s_TerrainLayerIdToGuidMap;

	static TerrainLayerId()
	{
		s_TerrainLayerGuidToIdMap = new Dictionary<TerrainLayerGuid, int>();
		s_TerrainLayerIdToGuidMap = new List<TerrainLayerGuid>();
		s_TerrainLayerGuidToIdMap.Add(default(TerrainLayerGuid), 0);
		s_TerrainLayerIdToGuidMap.Add(default(TerrainLayerGuid));
	}

	public static int GetTerrainLayerCount()
	{
		return s_TerrainLayerIdToGuidMap.Count;
	}

	public static int GetTerrainLayerId(TerrainLayerGuid guid)
	{
		if (s_TerrainLayerGuidToIdMap.TryGetValue(guid, out var value))
		{
			return value;
		}
		value = s_TerrainLayerIdToGuidMap.Count;
		s_TerrainLayerGuidToIdMap.Add(guid, value);
		s_TerrainLayerIdToGuidMap.Add(guid);
		return value;
	}

	public static TerrainLayerGuid GetTerrainLayerGuid(int terrainLayerId)
	{
		return s_TerrainLayerIdToGuidMap[terrainLayerId];
	}
}
