using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct StatResult
{
	public readonly int ModifiedValue;

	public readonly int BaseValue;

	public readonly StatType? BaseStat;

	public readonly StatType? FullOverrideStat;

	public StatResult(int modifiedValue, int baseValue, StatType? baseStat, StatType? fullOverrideStat)
	{
		ModifiedValue = modifiedValue;
		BaseValue = baseValue;
		BaseStat = baseStat;
		FullOverrideStat = fullOverrideStat;
	}

	public static implicit operator int(StatResult result)
	{
		return result.ModifiedValue;
	}
}
