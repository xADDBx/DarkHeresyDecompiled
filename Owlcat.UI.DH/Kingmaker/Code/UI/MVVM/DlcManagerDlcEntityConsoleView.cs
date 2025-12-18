namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerDlcEntityConsoleView : DlcManagerDlcEntityBaseView
{
	public bool IsDlcCanBeDeleted => base.ViewModel.IsDlcCanBeDeleted.CurrentValue;

	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}
}
