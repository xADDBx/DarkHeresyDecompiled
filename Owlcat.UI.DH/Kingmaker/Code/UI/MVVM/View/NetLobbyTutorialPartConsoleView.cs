using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyTutorialPartConsoleView : NetLobbyTutorialPartBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "NetLobbyTutorial"
		};
		m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowBlock();
		}, 8), UIStrings.Instance.CommonTexts.Accept).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
