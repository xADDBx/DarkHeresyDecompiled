using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class DatabaseLayerMapping
{
	private readonly int[] m_TerrainLayerIdToDatabaseLayerIndexMap;

	public DatabaseLayerMapping(TerrainLayerGuid[] terrainLayerGuids)
	{
		List<int> value;
		using (CollectionPool<List<int>, int>.Get(out value))
		{
			foreach (TerrainLayerGuid guid in terrainLayerGuids)
			{
				value.Add(TerrainLayerId.GetTerrainLayerId(guid));
			}
			m_TerrainLayerIdToDatabaseLayerIndexMap = new int[TerrainLayerId.GetTerrainLayerCount()];
			Array.Fill(m_TerrainLayerIdToDatabaseLayerIndexMap, -1);
			for (int j = 0; j < terrainLayerGuids.Length; j++)
			{
				m_TerrainLayerIdToDatabaseLayerIndexMap[value[j]] = j;
			}
		}
	}

	public bool TryGetDatabaseLayerIndex(int terranLayerId, out int result)
	{
		if (terranLayerId < m_TerrainLayerIdToDatabaseLayerIndexMap.Length)
		{
			result = m_TerrainLayerIdToDatabaseLayerIndexMap[terranLayerId];
			return true;
		}
		result = -1;
		return false;
	}
}
