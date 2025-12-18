using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Vendor;

public class VendorBaseScreenView : View<VendorBaseScreenVM>, IInitializable
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	protected VendorTradeView m_VendorTradePartView;

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

	[SerializeField]
	protected GameObject m_ReputationButtonObject;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_VendorTradePartView.Initialize();
		m_ReputationView.Initialize();
		m_VendorButtonTitle.text = UIStrings.Instance.Vendor.Vendor;
		m_ReputationButtonTitle.text = UIStrings.Instance.Vendor.Reputation;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		EventBus.Subscribe(this).AddTo(this);
		m_VendorTabNavigation.Bind(base.ViewModel.VendorTabNavigationVM);
		m_SelectorView.Bind(base.ViewModel.Selector);
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		base.ViewModel.ActiveTab.Subscribe(BindSelectedView).AddTo(this);
		m_ReputationButtonObject.SetActive(VendorHelper.TradeLogic.VendorFactionType != FactionType.None);
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
