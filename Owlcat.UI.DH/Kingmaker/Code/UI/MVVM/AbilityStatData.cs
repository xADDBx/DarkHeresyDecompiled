using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Code.UI.MVVM;

public readonly struct AbilityStatData
{
	public readonly StatType StatType;

	public readonly string StatName;

	public readonly int StatValue;

	public AbilityStatData(StatType statType, string statName, int statValue)
	{
		StatType = statType;
		StatName = statName;
		StatValue = statValue;
	}
}
