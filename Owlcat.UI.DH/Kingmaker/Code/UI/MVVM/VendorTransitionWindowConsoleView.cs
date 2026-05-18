namespace Kingmaker.Code.UI.MVVM;

public class VendorTransitionWindowConsoleView : VendorTransitionWindowView
{
	private void CreateInput()
	{
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Close();
	}

	private void ChangeSliderValue(int value)
	{
		m_Slider.value += value;
	}
}
