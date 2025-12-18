using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterSearchVM : ViewModel
{
	private readonly ReadOnlyReactiveProperty<string> m_SearchString;

	private readonly Action<string> m_SetSearchQuery;

	public ItemsFilterSearchVM(ReadOnlyReactiveProperty<string> searchString, Action<string> setSearch)
	{
		m_SearchString = searchString;
		m_SetSearchQuery = setSearch;
	}

	public void SetSearchString(string value)
	{
		m_SetSearchQuery(value);
	}
}
