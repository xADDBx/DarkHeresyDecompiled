using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationForItemWindowConsoleView : VendorReputationForItemWindowView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	public void ForceScrollToTop()
	{
		m_VirtualList.ScrollController.ForceScrollToTop();
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
		m_NavigationBehaviour.AddEntityGrid(m_VirtualList.GetNavigationBehaviour());
		return m_NavigationBehaviour;
	}
}
