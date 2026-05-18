using Kingmaker.Code.Gameplay.Components;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMoraleVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityUIState;

	public readonly UnitMoraleVM MoraleVM;

	public OvertipMoraleVM(MechanicEntityUIState entityUIState)
	{
		m_EntityUIState = entityUIState;
		MoraleVM = new UnitMoraleVM(entityUIState).AddTo(this);
	}

	public bool IsVisible()
	{
		if (MoraleVM.IsVisible())
		{
			return !m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Morale);
		}
		return false;
	}
}
