using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using R3;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoCharacteristicsTabVM : CharInfoComponentVM
{
	public readonly CharInfoSkillsAndWeaponsVM CharInfoSkillsAndWeaponsVM;

	public readonly ReactiveProperty<CharInfoStatusEffectsVM> StatusEffects = new ReactiveProperty<CharInfoStatusEffectsVM>();

	public CharInfoCharacteristicsTabVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		CharInfoSkillsAndWeaponsVM = new CharInfoSkillsAndWeaponsVM(Unit).AddTo(this);
		StatusEffects.Value = new CharInfoStatusEffectsVM(unit).AddTo(this);
	}
}
