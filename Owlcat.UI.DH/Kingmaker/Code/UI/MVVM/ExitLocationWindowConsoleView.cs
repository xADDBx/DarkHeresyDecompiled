using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExitLocationWindowConsoleView : ExitLocationWindowBaseView
{
	[SerializeField]
	protected ConsoleHint m_AcceptHint;

	[SerializeField]
	protected ConsoleHint m_DeclineHint;

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
			ContextName = "Exit Location Window"
		};
		m_DeclineHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Decline();
		}, 9)).AddTo(this);
		m_DeclineHint.SetLabel(DeclineText.text);
		m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Confirm();
		}, 8)).AddTo(this);
		m_AcceptHint.SetLabel(AcceptText.text);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
