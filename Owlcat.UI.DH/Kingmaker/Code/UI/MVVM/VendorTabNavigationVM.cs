using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTabNavigationVM : ViewModel
{
	private readonly ReactiveProperty<VendorWindowsTab> m_ActiveTab = new ReactiveProperty<VendorWindowsTab>(VendorWindowsTab.Trade);

	public ReadOnlyReactiveProperty<VendorWindowsTab> ActiveTab => m_ActiveTab;

	public void SetActiveTab(VendorWindowsTab activeTab)
	{
		if (ActiveTab.CurrentValue != activeTab)
		{
			m_ActiveTab.Value = activeTab;
		}
	}

	public void SetNextTab()
	{
		if (ActiveTab.CurrentValue == VendorWindowsTab.Trade)
		{
			SetActiveTab(VendorWindowsTab.Reputation);
		}
		else
		{
			SetActiveTab(VendorWindowsTab.Trade);
		}
	}

	public void OnNextActiveTab()
	{
		VendorWindowsTab vendorWindowsTab = ActiveTab.CurrentValue + 1;
		if (vendorWindowsTab >= VendorWindowsTab.Trade && vendorWindowsTab <= VendorWindowsTab.Trade)
		{
			SetActiveTab(vendorWindowsTab);
		}
	}

	public void OnPrevActiveTab()
	{
		VendorWindowsTab vendorWindowsTab = ActiveTab.CurrentValue - 1;
		if (vendorWindowsTab >= VendorWindowsTab.Trade && vendorWindowsTab <= VendorWindowsTab.Trade)
		{
			SetActiveTab(vendorWindowsTab);
		}
	}
}
