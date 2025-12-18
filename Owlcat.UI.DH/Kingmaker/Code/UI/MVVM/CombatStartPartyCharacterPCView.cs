namespace Kingmaker.Code.UI.MVVM;

public class CombatStartPartyCharacterPCView : PartyCharacterBaseView
{
	protected override void OnSingleLeftClick()
	{
		base.OnSingleLeftClick();
		base.ViewModel.HandleUnitClick(isDoubleClick: true);
	}

	protected override void OnDoubleLeftClick()
	{
		base.OnDoubleLeftClick();
		base.ViewModel.HandleUnitClick(isDoubleClick: true);
	}
}
