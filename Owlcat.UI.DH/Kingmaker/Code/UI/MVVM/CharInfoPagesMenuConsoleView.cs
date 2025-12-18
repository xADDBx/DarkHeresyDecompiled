using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPagesMenuConsoleView : CharInfoPagesMenuPCView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	public void AddHints(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		m_PreviousFilterHint.Bind(inputLayer.AddButton(delegate
		{
			SelectPrev();
		}, 14, enabledHints)).AddTo(this);
		m_NextFilterHint.Bind(inputLayer.AddButton(delegate
		{
			SelectNext();
		}, 15, enabledHints)).AddTo(this);
	}
}
