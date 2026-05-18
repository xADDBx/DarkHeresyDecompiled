using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct AffectedStatEntry
{
	public readonly StatType Stat;

	public readonly bool IsConditional;

	public readonly StatType[]? DependsOnStats;

	public AffectedStatEntry(StatType stat, bool isConditional = false)
	{
		Stat = stat;
		IsConditional = isConditional;
		DependsOnStats = null;
	}

	public AffectedStatEntry(StatType stat, params StatType[] dependsOnStats)
	{
		Stat = stat;
		IsConditional = true;
		DependsOnStats = dependsOnStats;
	}

	public static AffectedStatEntry Create(StatType stat, bool restrictionsEmpty, StatType[]? dependsOnStats)
	{
		if (dependsOnStats != null && dependsOnStats.Length > 0)
		{
			return new AffectedStatEntry(stat, dependsOnStats);
		}
		if (restrictionsEmpty)
		{
			return new AffectedStatEntry(stat);
		}
		return new AffectedStatEntry(stat, isConditional: true);
	}
}
