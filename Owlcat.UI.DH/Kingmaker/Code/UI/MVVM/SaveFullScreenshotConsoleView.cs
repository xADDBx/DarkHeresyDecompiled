namespace Kingmaker.Code.UI.MVVM;

public class SaveFullScreenshotConsoleView : SaveFullScreenshotBaseView
{
	public const string InputLayerContextName = "SaveFullScreenshotConsoleView";

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
	}
}
