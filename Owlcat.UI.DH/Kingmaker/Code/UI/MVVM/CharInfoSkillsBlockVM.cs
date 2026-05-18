using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsBlockVM : CharInfoBaseAbilityScoresBlockVM
{
	public static List<StatType> SkillsOrdered = new List<StatType>
	{
		StatType.SkillAthletics,
		StatType.SkillTenacity,
		StatType.SkillResistance,
		StatType.SkillDemolition,
		StatType.SkillReflexes,
		StatType.SkillSleightOfHand,
		StatType.SkillMobility,
		StatType.SkillLoreHeresy,
		StatType.SkillLoreXenos,
		StatType.SkillLoreWarp,
		StatType.SkillTechUse,
		StatType.SkillInterrogation,
		StatType.SkillMettle,
		StatType.SkillAwareness,
		StatType.SkillWits,
		StatType.SkillIntimidation,
		StatType.SkillDiplomacy,
		StatType.SkillMedicae
	};

	protected override List<StatType> StatsTypes { get; } = SkillsOrdered;


	public CharInfoSkillsBlockVM(BaseUnitEntity unit)
		: base(unit)
	{
	}

	public CharInfoSkillsBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
	}
}
