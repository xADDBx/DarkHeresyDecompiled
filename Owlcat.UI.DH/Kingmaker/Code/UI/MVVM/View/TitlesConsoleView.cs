using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TitlesConsoleView : TitlesBaseView
{
	[SerializeField]
	private HintView m_SpeedUPHint;

	[SerializeField]
	private HintView m_CloseHint;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
	}
}
