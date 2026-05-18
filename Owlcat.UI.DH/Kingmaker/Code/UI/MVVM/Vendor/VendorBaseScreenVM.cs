using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Vendor;

public class VendorBaseScreenVM : ViewModel
{
	private readonly ReactiveProperty<VendorWindowsTab> m_ActiveTab = new ReactiveProperty<VendorWindowsTab>();

	public readonly LensSelectorVM Selector;

	public readonly VendorTabNavigationVM VendorTabNavigationVM;

	public ReadOnlyReactiveProperty<VendorWindowsTab> ActiveTab => m_ActiveTab;

	public VendorTradeViewVM VendorTradePartVM { get; private set; }

	public FactionReputationVM FactionReputationVM { get; private set; }

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
		if (VendorTradePartVM == null && FactionReputationVM == null)
		{
			FullScreenSounds.Instance.Vendor.Open.Play();
		}
		else
		{
			FullScreenSounds.Instance.Vendor.SwitchTo.Play();
		}
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			VendorTradePartVM?.Dispose();
			FactionReputationVM?.Dispose();
			VendorTradePartVM = new VendorTradeViewVM(VendorHelper.TradeLogic.VendorEntity, delegate
			{
				VendorTradePartVM?.Dispose();
			}).AddTo(this);
			break;
		case VendorWindowsTab.Reputation:
			VendorTradePartVM?.Dispose();
			FactionReputationVM = new FactionReputationVM().AddTo(this);
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
		FullScreenSounds.Instance.Vendor.Close.Play();
	}
}
