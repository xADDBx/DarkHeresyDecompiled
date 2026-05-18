using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Pathfinding;

public class AdditionalGraphDataManager
{
	private Dictionary<uint, AdditionalGraphData> m_PerGraphData = new Dictionary<uint, AdditionalGraphData>();

	public static AdditionalGraphDataManager Instance = new AdditionalGraphDataManager();

	public void SetGraphData(uint graphIndex, AdditionalGraphData data)
	{
		if (m_PerGraphData.TryGetValue(graphIndex, out var value))
		{
			value.Dispose();
		}
		m_PerGraphData.Remove(graphIndex);
		m_PerGraphData.Add(graphIndex, data);
	}

	[CanBeNull]
	public AdditionalGraphData GetGraphData(uint graphIndex)
	{
		return GetGraphDataOptional(graphIndex);
	}

	[CanBeNull]
	public AdditionalGraphData GetGraphDataOptional(uint graphIndex)
	{
		return m_PerGraphData.GetValueOrDefault(graphIndex);
	}

	[NotNull]
	public AdditionalGraphData GetGridData()
	{
		return GetGridDataOptional() ?? throw new InvalidOperationException("Grid graph is missing");
	}

	[CanBeNull]
	public AdditionalGraphData GetGridDataOptional()
	{
		uint? num = AstarPath.active?.data.gridGraph?.graphIndex;
		if (num.HasValue)
		{
			uint valueOrDefault = num.GetValueOrDefault();
			return GetGraphData(valueOrDefault);
		}
		return null;
	}
}
