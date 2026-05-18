using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.UnitDescription;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityScoresBlockVM : CharInfoBaseAbilityScoresBlockVM
{
	public static List<StatType> AbilitiesOrdered = new List<StatType>
	{
		StatType.BallisticSkill,
		StatType.WeaponSkill,
		StatType.Strength,
		StatType.Toughness,
		StatType.Agility,
		StatType.Intelligence,
		StatType.Willpower,
		StatType.Perception,
		StatType.Fellowship
	};

	protected override List<StatType> StatsTypes { get; } = AbilitiesOrdered;


	public CharInfoAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
	}

	public CharInfoAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit, null)
	{
	}

	public CharInfoAbilityScoresBlockVM(UnitDescription.StatsData statsData)
	{
	}
}
