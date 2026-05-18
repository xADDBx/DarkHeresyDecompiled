using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotConsoleView : SaveSlotBaseView, IFunc01ClickHandler, IConsoleEntity, IFunc02ClickHandler
{
	public bool CanFunc01Click()
	{
		return base.ViewModel != null;
	}

	public string GetFunc01ClickHint()
	{
		return string.Empty;
	}

	public void OnFunc01Click()
	{
		HandleFunc01Click();
	}

	public bool CanFunc02Click()
	{
		return base.ViewModel != null;
	}

	public string GetFunc02ClickHint()
	{
		return string.Empty;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.ShowScreenshot();
	}

	protected virtual void HandleFunc01Click()
	{
		if (base.ViewModel.IsActuallySaved)
		{
			base.ViewModel.Delete();
		}
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
		m_Button.CanConfirm = true;
	}
}
