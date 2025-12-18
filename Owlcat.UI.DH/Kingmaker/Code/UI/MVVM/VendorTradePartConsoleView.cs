using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTradePartConsoleView : VendorTradePartView<ItemsFilterConsoleView, VendorLevelItemsConsoleView, VendorTransitionWindowConsoleView>
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour2 = new GridConsoleNavigationBehaviour();
		List<IBindable> entries = m_WidgetList.Entries;
		if (entries != null && entries.Count > 0)
		{
			gridConsoleNavigationBehaviour2.SetEntitiesVertical(m_WidgetList.Entries?.Select((IBindable e) => (e as VendorLevelItemsConsoleView)?.GetNavigation()).ToList() ?? throw new InvalidOperationException());
		}
		m_NavigationBehaviour.SetEntitiesHorizontal<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour, gridConsoleNavigationBehaviour2);
		m_NavigationBehaviour.FocusOnEntityManual(gridConsoleNavigationBehaviour2.Entities.Any((IConsoleEntity x) => x.IsValid()) ? gridConsoleNavigationBehaviour2 : gridConsoleNavigationBehaviour);
		return m_NavigationBehaviour;
	}
}
