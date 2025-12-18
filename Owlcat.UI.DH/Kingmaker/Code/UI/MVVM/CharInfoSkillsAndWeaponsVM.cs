using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsAndWeaponsVM : CharInfoComponentWithLevelUpVM
{
	public readonly CharInfoAbilityScoresBlockVM AbilityScoresBlockVM;

	public readonly CharInfoSkillsBlockVM SkillsBlockVM;

	public readonly CharInfoWeaponsBlockVM WeaponsBlockVM;

	public CharInfoSkillsAndWeaponsVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		AbilityScoresBlockVM = new CharInfoAbilityScoresBlockVM(unit).AddTo(this);
		SkillsBlockVM = new CharInfoSkillsBlockVM(unit, levelUpManager).AddTo(this);
		WeaponsBlockVM = new CharInfoWeaponsBlockVM(unit).AddTo(this);
	}
}
