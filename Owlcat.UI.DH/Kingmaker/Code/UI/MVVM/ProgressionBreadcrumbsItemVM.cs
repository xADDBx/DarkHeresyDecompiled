using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ProgressionBreadcrumbsItemVM : ViewModel
{
	public readonly UnitProgressionWindowState ProgressionState;

	public readonly string Text;

	public readonly bool IsCurrent;

	private readonly Action<UnitProgressionWindowState> m_SetStateAction;

	public ProgressionBreadcrumbsItemVM(UnitProgressionWindowState progressionState, string text, bool isCurrent, Action<UnitProgressionWindowState> setStateAction)
	{
		ProgressionState = progressionState;
		Text = text;
		IsCurrent = isCurrent;
		m_SetStateAction = setStateAction;
	}

	public void HandleClick()
	{
		m_SetStateAction?.Invoke(ProgressionState);
	}
}
