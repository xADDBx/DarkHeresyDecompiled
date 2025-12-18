using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Vendor;

public class VendorBaseScreenVM : ViewModel
{
	private readonly ReactiveProperty<VendorWindowsTab> m_ActiveTab = new ReactiveProperty<VendorWindowsTab>();

	public readonly LensSelectorVM Selector;

	public readonly VendorTabNavigationVM VendorTabNavigationVM;

	public VendorTradeViewVM VendorTradePartVM;

	public FactionReputationVM FactionReputationVM;

	public ReadOnlyReactiveProperty<VendorWindowsTab> ActiveTab => m_ActiveTab;

	public VendorBaseScreenVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Vendor);
		});
		VendorTabNavigationVM = new VendorTabNavigationVM().AddTo(this);
		VendorTabNavigationVM.ActiveTab.Subscribe(SelectWindow).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
	}

	private void SelectWindow(VendorWindowsTab tab)
	{
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			VendorTradePartVM?.Dispose();
			FactionReputationVM?.Dispose();
			VendorTradePartVM = new VendorTradeViewVM(VendorHelper.TradeLogic.VendorEntity, delegate
			{
				VendorTradePartVM?.Dispose();
			});
			break;
		case VendorWindowsTab.Reputation:
			VendorTradePartVM?.Dispose();
			FactionReputationVM = new FactionReputationVM();
			break;
		}
		m_ActiveTab.Value = tab;
	}

	public void Close()
	{
		VendorTradePartVM.Close();
		FactionReputationVM?.Dispose();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Vendor);
		});
	}
}
