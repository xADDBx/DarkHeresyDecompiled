using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;

namespace Kingmaker.Blueprints;

[Serializable]
public class BurstWeightSettings
{
	[Serializable]
	public class BurstUnitWeight
	{
		public Size Size;

		public int Weight;
	}

	public int MissWeight;

	public int DestructibleWeight;

	public List<BurstUnitWeight> UnitWeights = new List<BurstUnitWeight>();

	private Dictionary<Size, int> m_UnitWeightsMap;

	public int GetEntityWeight(MechanicEntity entity)
	{
		if (m_UnitWeightsMap == null)
		{
			m_UnitWeightsMap = UnitWeights.ToDictionary((BurstUnitWeight x) => x.Size, (BurstUnitWeight x) => x.Weight);
		}
		if (entity is DestructibleEntity)
		{
			return DestructibleWeight;
		}
		if (entity is BaseUnitEntity baseUnitEntity && m_UnitWeightsMap.TryGetValue(baseUnitEntity.Size, out var value))
		{
			return value;
		}
		return 0;
	}
}
