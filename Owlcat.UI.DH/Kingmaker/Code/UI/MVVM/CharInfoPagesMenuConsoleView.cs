using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPagesMenuConsoleView : CharInfoPagesMenuPCView
{
	[Header("Hints")]
	[SerializeField]
	private HintView m_PreviousFilterHint;

	[SerializeField]
	private HintView m_NextFilterHint;

	public void AddHints()
	{
	}
}
