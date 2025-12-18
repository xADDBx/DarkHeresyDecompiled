using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class NodeTraverseCostHelper
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("NodeTraverseCostHelper");

	private static readonly List<OverrideCostData> OverrideCosts = new List<OverrideCostData>();

	public static Dictionary<GraphNode, float> GetOverrideCosts(AbstractUnitEntity entity)
	{
		Dictionary<GraphNode, float> result = new Dictionary<GraphNode, float>();
		CollectOverrideCosts(entity, result);
		return result;
	}

	public static void CollectOverrideCosts(AbstractUnitEntity entity, Dictionary<GraphNode, float> result)
	{
		if (!entity.IsInCombat || !(entity is BaseUnitEntity unitEntity))
		{
			return;
		}
		foreach (OverrideCostData overrideCost in OverrideCosts)
		{
			if (!overrideCost.IsCorrectUnit(unitEntity))
			{
				continue;
			}
			foreach (GridNodeBase node in overrideCost.Nodes)
			{
				result[node] = (float)overrideCost.OverridePercentCost / 100f;
			}
		}
	}

	public static void AddOverrideCost(EntityFactSource source, int overridePercentCost, RestrictionsHolder.Reference restrictions)
	{
		if (OverrideCosts.Any((OverrideCostData x) => x.Source == source && x.OverridePercentCost == overridePercentCost))
		{
			Logger.Log($"Cost already overriden by {source}");
			return;
		}
		OverrideCostData overrideCostData = new OverrideCostData(source, overridePercentCost, restrictions);
		OverrideCosts.Add(overrideCostData);
		Logger.Log($"Add override cost {overrideCostData}");
	}

	public static void RemoveOverrideCost(EntityFactSource source)
	{
		if (OverrideCosts.RemoveAll((OverrideCostData x) => x.Source == source) > 0)
		{
			Logger.Log($"Remove override cost from {source}");
		}
		else
		{
			Logger.Log($"No cost overrides from {source}");
		}
	}
}
