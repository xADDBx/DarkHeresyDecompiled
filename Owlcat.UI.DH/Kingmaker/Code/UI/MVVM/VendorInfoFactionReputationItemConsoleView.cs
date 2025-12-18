using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorInfoFactionReputationItemConsoleView : CharInfoFactionReputationItemConsoleView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool HasVendors
	{
		get
		{
			if (m_WidgetList != null)
			{
				return m_WidgetList.Entries.Count > 0;
			}
			return false;
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
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
		gridConsoleNavigationBehaviour.AddEntityHorizontal(this);
		m_NavigationBehaviour.AddEntityGrid(gridConsoleNavigationBehaviour);
		return m_NavigationBehaviour;
	}

	public void TryTrade()
	{
		if (HasVendors && m_WidgetList.Entries.FirstOrDefault((IBindable x) => x as FactionVendorInformationBaseView) is FactionVendorInformationBaseView factionVendorInformationBaseView)
		{
			factionVendorInformationBaseView.StartTrade();
		}
	}
}
