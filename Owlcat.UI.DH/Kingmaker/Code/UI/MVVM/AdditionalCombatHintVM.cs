using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class AdditionalCombatHintVM : ViewModel
{
	public readonly List<CombatHintEntityVM> CombatObjectives;

	public AdditionalCombatHintVM(List<BlueprintAdditionalCombatObjective> combatObjective, ReactiveProperty<OvertipState> currentState)
	{
		CombatObjectives = combatObjective.ConvertAll((BlueprintAdditionalCombatObjective c) => new CombatHintEntityVM(c, currentState));
	}
}
