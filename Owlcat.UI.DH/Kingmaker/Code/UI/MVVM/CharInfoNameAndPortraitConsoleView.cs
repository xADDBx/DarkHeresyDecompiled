using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoNameAndPortraitConsoleView : CharInfoNameAndPortraitBaseView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectPrevCharacter();
		}, 14, enabledHints);
		m_PreviousFilterHint.Bind(inputBindStruct).AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectNextCharacter();
		}, 15, enabledHints);
		m_NextFilterHint.Bind(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
	}
}
