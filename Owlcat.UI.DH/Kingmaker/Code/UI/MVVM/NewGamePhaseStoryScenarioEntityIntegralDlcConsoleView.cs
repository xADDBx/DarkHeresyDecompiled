namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioEntityIntegralDlcConsoleView : NewGamePhaseStoryScenarioEntityIntegralDlcBaseView
{
	protected override void OnChangeSelectedStateImpl()
	{
		base.OnChangeSelectedStateImpl();
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}
}
