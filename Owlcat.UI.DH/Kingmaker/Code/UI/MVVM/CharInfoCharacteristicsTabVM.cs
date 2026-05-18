using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoCharacteristicsTabVM : CharInfoComponentVM
{
	public readonly CharInfoSkillsAndWeaponsVM CharInfoSkillsAndWeaponsVM;

	public readonly ReactiveProperty<CharInfoStatusEffectsVM> StatusEffects = new ReactiveProperty<CharInfoStatusEffectsVM>();

	public CharInfoCharacteristicsTabVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, BuffGroupsVM buffGroupsVM)
		: base(unit)
	{
		CharInfoSkillsAndWeaponsVM = new CharInfoSkillsAndWeaponsVM(Unit).AddTo(this);
		StatusEffects.Value = new CharInfoStatusEffectsVM(unit, buffGroupsVM).AddTo(this);
	}
}
