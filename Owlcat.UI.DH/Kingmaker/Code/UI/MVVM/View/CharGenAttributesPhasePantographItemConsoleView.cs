using Kingmaker.Code.View.Bridge.OBSOLETE;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhasePantographItemConsoleView : CharGenAttributesPhasePantographItemView
{
	[SerializeField]
	private ConsoleHint m_MinusHint;

	[SerializeField]
	private ConsoleHint m_PlusHint;

	private bool m_HintsAdded;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_HintsAdded = false;
		AddHints(base.ViewModel.IsMainCharacter.CurrentValue);
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(AddHints));
	}

	private void AddHints(bool isMainCharacter)
	{
		if (isMainCharacter && !m_HintsAdded)
		{
			AddDisposable(DelayedInvoker.InvokeInFrames(delegate
			{
				AddDisposable(m_MinusHint.BindCustomAction(4, GamePad.Instance.CurrentInputLayer, base.ViewModel.CanRetreat));
				AddDisposable(m_PlusHint.BindCustomAction(5, GamePad.Instance.CurrentInputLayer, base.ViewModel.CanAdvance));
			}, 5));
			m_HintsAdded = true;
		}
	}
}
