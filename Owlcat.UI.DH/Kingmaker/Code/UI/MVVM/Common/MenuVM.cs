using System.Collections.Generic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Common;

public class MenuVM : SelectionGroupRadioVM<MenuEntityVM>
{
	public readonly LensSelectorVM Selector;

	public MenuVM(List<MenuEntityVM> visibleCollection, ReactiveProperty<MenuEntityVM> entity = null, bool cyclical = false)
		: base(visibleCollection, entity, cyclical)
	{
		Selector = new LensSelectorVM().AddTo(this);
	}

	public void DisableAllExcept(int enumId)
	{
		bool flag = enumId == 0;
		foreach (MenuEntityVM item in EntitiesCollection)
		{
			item.SetAvailable(flag || item.EnumId == enumId);
		}
	}
}
