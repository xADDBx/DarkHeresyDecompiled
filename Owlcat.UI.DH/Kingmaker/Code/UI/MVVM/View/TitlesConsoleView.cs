using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TitlesConsoleView : TitlesBaseView
{
	[SerializeField]
	private ConsoleHint m_SpeedUPHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "TitlesView"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OpenCancelSettingsDialog();
		}, 9, InputActionEventType.ButtonJustLongPressed);
		inputBindStruct.AddTo(this);
		m_CloseHint.Bind(inputBindStruct).AddTo(this);
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.SkipHold);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: true);
		}, 8);
		inputBindStruct2.AddTo(this);
		m_SpeedUPHint.Bind(inputBindStruct2).AddTo(this);
		m_SpeedUPHint.SetLabel(UIStrings.Instance.CommonTexts.HoldGamepadButton.Text + " " + UIStrings.Instance.Credits.SpeedUp.Text);
		m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 8, InputActionEventType.ButtonJustReleased).AddTo(this);
		m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 8, InputActionEventType.ButtonLongPressJustReleased).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
