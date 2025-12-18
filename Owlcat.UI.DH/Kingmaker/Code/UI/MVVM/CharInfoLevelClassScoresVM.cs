using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoLevelClassScoresVM : CharInfoComponentWithLevelUpVM
{
	public CharInfoExperienceVM Experience { get; }

	public CharInfoClassesListVM Classes { get; }

	public CharInfoAbilityScoresBlockVM AbilityScores { get; }

	public InventoryDollAdditionalStatsVM AdditionalStatsVM { get; }

	public CharInfoGlossaryStatVM CohesionStatVM { get; }

	public CharInfoLevelClassScoresVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		Experience = new CharInfoExperienceVM(unit).AddTo(this);
		Classes = new CharInfoClassesListVM(unit).AddTo(this);
		AbilityScores = new CharInfoAbilityScoresBlockVM(unit, levelUpManager).AddTo(this);
		AdditionalStatsVM = new InventoryDollAdditionalStatsVM(unit, levelUpManager).AddTo(this);
		CohesionStatVM = new CharInfoGlossaryStatVM(unit, StatType.CohesionRange);
	}
}
