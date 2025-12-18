using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

public class RegularGrid<T>
{
	private Dictionary<int3, List<T>> m_GridMap = new Dictionary<int3, List<T>>();

	private float m_CellSize;

	private Func<T, float3> m_PositionGetter;

	public RegularGrid(float cellSize, Func<T, float3> positionGetter)
	{
		m_CellSize = cellSize;
		if (positionGetter == null)
		{
			m_PositionGetter = (T item) => float3.zero;
		}
		else
		{
			m_PositionGetter = positionGetter;
		}
	}

	public int3 GetCellCoords(float3 position)
	{
		return (int3)math.floor(position / m_CellSize);
	}

	public void AddElement(T element)
	{
		int3 cellCoords = GetCellCoords(m_PositionGetter(element));
		if (m_GridMap.TryGetValue(cellCoords, out var value))
		{
			value.Add(element);
			return;
		}
		m_GridMap[cellCoords] = new List<T> { element };
	}

	public bool RemoveElement(T element)
	{
		int3 cellCoords = GetCellCoords(m_PositionGetter(element));
		if (m_GridMap.TryGetValue(cellCoords, out var value))
		{
			return value.Remove(element);
		}
		return false;
	}

	public IEnumerable<T> GetNeighborsEnumerator(T element)
	{
		if (m_CellSize < 1E-06f)
		{
			yield break;
		}
		float3 position = m_PositionGetter(element);
		int3 coords = GetCellCoords(position);
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				for (int z = -1; z <= 1; z++)
				{
					int3 key = coords + new int3(x, y, z);
					if (!m_GridMap.TryGetValue(key, out var value))
					{
						continue;
					}
					foreach (T item in value)
					{
						if (!item.Equals(element) && math.distance(position, m_PositionGetter(item)) < m_CellSize)
						{
							yield return item;
						}
					}
				}
			}
		}
	}

	public IEnumerable<T> GetNeighborsEnumerator(float3 position)
	{
		if (m_CellSize < 1E-06f)
		{
			yield break;
		}
		int3 coords = GetCellCoords(position);
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				for (int z = -1; z <= 1; z++)
				{
					int3 key = coords + new int3(x, y, z);
					if (!m_GridMap.TryGetValue(key, out var value))
					{
						continue;
					}
					foreach (T item in value)
					{
						if (math.distance(position, m_PositionGetter(item)) < m_CellSize)
						{
							yield return item;
						}
					}
				}
			}
		}
	}
}
