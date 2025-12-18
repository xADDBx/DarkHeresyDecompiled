using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSlotConsoleView : VendorSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	public void SetFocus(bool value)
	{
		m_ItemSlotView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_ItemSlotView != null;
	}

	public void OnConfirmClick()
	{
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
