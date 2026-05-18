using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoNameAndPortraitConsoleView : CharInfoNameAndPortraitBaseView
{
	[Header("Console")]
	[SerializeField]
	private HintView m_PreviousFilterHint;

	[SerializeField]
	private HintView m_NextFilterHint;

	public void AddInput()
	{
	}
}
