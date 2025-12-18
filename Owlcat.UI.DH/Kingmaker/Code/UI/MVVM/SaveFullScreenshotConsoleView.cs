using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveFullScreenshotConsoleView : SaveFullScreenshotBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	public const string InputLayerContextName = "SaveFullScreenshotConsoleView";

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "SaveFullScreenshotConsoleView"
		};
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.HideScreenshot();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
