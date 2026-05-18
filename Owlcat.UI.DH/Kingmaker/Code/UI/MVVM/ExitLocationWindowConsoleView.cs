using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExitLocationWindowConsoleView : ExitLocationWindowBaseView
{
	[SerializeField]
	protected HintView m_AcceptHint;

	[SerializeField]
	protected HintView m_DeclineHint;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
	}
}
