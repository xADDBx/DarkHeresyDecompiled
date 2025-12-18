using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorLevelItemsConsoleView : VendorLevelItemsBaseView
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
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Cast<VendorSlotConsoleView>().ToList(), 1);
		return m_NavigationBehaviour;
	}
}
