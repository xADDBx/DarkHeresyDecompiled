namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageImageConsoleView : EncyclopediaPageImageBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_ZoomButton.gameObject.SetActive(value: false);
	}
}
