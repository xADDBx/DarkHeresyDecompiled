using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoAbilityVM : ViewModel
{
	public readonly AbilityUIGroup Group;

	public readonly AbilityIconVM AbilityIconVM;

	public UnitInfoAbilityVM(BlueprintAbility abilityBlueprint, AbilityUIGroup group)
	{
		Group = group;
		AbilityIconVM = new AbilityIconVM(abilityBlueprint).AddTo(this);
	}

	public UnitInfoAbilityVM(BlueprintFeature featureBlueprint, AbilityUIGroup group)
	{
		Group = group;
		AbilityIconVM = new AbilityIconVM(featureBlueprint).AddTo(this);
	}
}
