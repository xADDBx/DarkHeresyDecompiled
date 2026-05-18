using Kingmaker.Code.Gameplay.Components;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityUIState;

	public UnitBuffBlockVM UnitBuffBlockVM { get; }

	public ReadOnlyReactiveProperty<bool> IsVisible { get; }

	public OvertipBuffBlockVM(MechanicEntity target, UnitBuffBlockVM buffBlockVM)
	{
		m_EntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(target);
		UnitBuffBlockVM = buffBlockVM;
		IsVisible = m_EntityUIState.IsInCombat.CombineLatest(m_EntityUIState.IsDeadOrUnconsciousIsDead, IsBuffsBlockVisible).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
	}

	private bool IsBuffsBlockVisible(bool isInCombat, bool isDead)
	{
		if (!isInCombat || m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Buffs))
		{
			return false;
		}
		return !isDead;
	}
}
