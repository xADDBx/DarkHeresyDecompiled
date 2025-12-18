using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatHintEntityVM : ViewModel
{
	public readonly BlueprintAdditionalCombatObjective CombatObjective;

	private readonly ReactiveProperty<OvertipState> m_CurrentState;

	public ReadOnlyReactiveProperty<OvertipState> CurrentState => m_CurrentState;

	public CombatHintEntityVM(BlueprintAdditionalCombatObjective combatObjective, ReactiveProperty<OvertipState> currentState)
	{
		CombatObjective = combatObjective;
		m_CurrentState = currentState;
	}
}
