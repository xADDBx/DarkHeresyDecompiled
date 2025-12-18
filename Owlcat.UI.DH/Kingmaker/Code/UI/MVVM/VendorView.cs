using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorView<TStashView, TItemsFilter, TVendorSlot, TVendorTransitionWindow> : View<VendorVM>, IInitializable where TStashView : InventoryStashView where TItemsFilter : ItemsFilterBaseView where TVendorSlot : VendorLevelItemsBaseView where TVendorTransitionWindow : VendorTransitionWindowView
{
	[SerializeField]
	protected TStashView m_StashView;

	[SerializeField]
	protected VendorTradePartView<TItemsFilter, TVendorSlot, TVendorTransitionWindow> m_VendorTradePartView;

	[SerializeField]
	protected FactionReputationView m_ReputationView;

	[SerializeField]
	protected VendorTabNavigationPCView m_VendorTabNavigation;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	protected TextMeshProUGUI m_VendorButtonTitle;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationButtonTitle;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_StashView.Initialize();
		m_VendorTradePartView.Initialize();
		m_ReputationView.Initialize();
		m_VendorButtonTitle.text = UIStrings.Instance.Vendor.Vendor;
		m_ReputationButtonTitle.text = UIStrings.Instance.Vendor.Reputation;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_VendorTabNavigation.Bind(base.ViewModel.VendorTabNavigationVM);
		m_SelectorView.Bind(base.ViewModel.Selector);
		m_StashView.Bind(base.ViewModel.StashVM);
		base.ViewModel.ActiveTab.Subscribe(BindSelectedView).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void BindSelectedView(VendorWindowsTab tab)
	{
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			m_ReputationView.Unbind();
			m_VendorTradePartView.Bind(base.ViewModel.VendorTradePartVM);
			break;
		case VendorWindowsTab.Reputation:
			m_VendorTradePartView.Unbind();
			m_ReputationView.Bind(base.ViewModel.FactionReputationVM);
			break;
		}
	}
}
