namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerDlcEntityPCView : DlcManagerDlcEntityBaseView
{
	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		SetFocus(value);
	}
}
