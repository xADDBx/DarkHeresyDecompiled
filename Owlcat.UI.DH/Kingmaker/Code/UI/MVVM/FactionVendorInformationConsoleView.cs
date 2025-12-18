using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FactionVendorInformationConsoleView : FactionVendorInformationBaseView, IConfirmClickHandler, IConsoleEntity
{
	public bool CanConfirmClick()
	{
		return base.ViewModel.Vendor != null;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.StartTrade();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
