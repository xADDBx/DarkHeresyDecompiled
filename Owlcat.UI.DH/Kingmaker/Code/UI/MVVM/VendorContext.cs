using Kingmaker.Code.UI.MVVM.Vendor;
using Kingmaker.Code.View.UI.UIUtils;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorContext : ViewModel, ITradeStateChanged, ISubscriber<IMechanicEntity>, ISubscriber
{
	private readonly ReactiveProperty<VendorBaseScreenVM> m_VendorVM;

	public VendorContext(ReactiveProperty<VendorBaseScreenVM> vendorVM)
	{
		m_VendorVM = vendorVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_VendorVM.ClearDisposableValue();
	}

	void ITradeStateChanged.HandleBeginTrading()
	{
		m_VendorVM.ClearDisposableValue();
		m_VendorVM.Value = new VendorBaseScreenVM();
	}

	void ITradeStateChanged.HandleEndTrading()
	{
		m_VendorVM.ClearDisposableValue();
	}

	void ITradeStateChanged.HandleVendorAboutToTrading()
	{
	}
}
