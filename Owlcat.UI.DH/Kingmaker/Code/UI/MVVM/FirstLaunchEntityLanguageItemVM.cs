using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchEntityLanguageItemVM : ViewModel
{
	public readonly string Title;

	private readonly int m_Index;

	private readonly Action<int> m_SetSelected;

	public readonly ReadOnlyReactiveProperty<bool> IsSelected;

	public FirstLaunchEntityLanguageItemVM(string language, int index, Action<int> setSelected, Observable<int> selectedIndex)
	{
		m_Index = index;
		m_SetSelected = setSelected;
		IsSelected = selectedIndex.Select((int i) => i == m_Index).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
		Title = language;
	}

	public void SetSelected()
	{
		m_SetSelected?.Invoke(m_Index);
	}
}
