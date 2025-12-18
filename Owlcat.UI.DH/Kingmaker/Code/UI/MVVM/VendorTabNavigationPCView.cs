using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTabNavigationPCView : View<VendorTabNavigationVM>
{
	private const string SelectedTabLayer = "Selected";

	private const string NormalTabLayer = "Normal";

	[SerializeField]
	private OwlcatMultiButton m_VendorViewButton;

	[SerializeField]
	private OwlcatMultiButton m_ReputationViewButton;

	private readonly ReactiveProperty<VendorWindowsTab> m_CurrentTab = new ReactiveProperty<VendorWindowsTab>();

	private readonly ReactiveProperty<bool> m_IsReputation = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<VendorWindowsTab> CurrentTab => m_CurrentTab;

	public ReadOnlyReactiveProperty<bool> IsReputation => m_IsReputation;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_VendorViewButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetActiveTab(VendorWindowsTab.Trade);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ReputationViewButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetActiveTab(VendorWindowsTab.Reputation);
		}).AddTo(this);
		base.ViewModel.ActiveTab.AsObservable().Subscribe(UpdateActiveTabVisual).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		if (VendorHelper.TradeLogic.VendorFactionType == FactionType.None)
		{
			m_ReputationViewButton.gameObject.SetActive(value: false);
		}
	}

	private void UpdateActiveTabVisual(VendorWindowsTab activeTab)
	{
		m_VendorViewButton.SetActiveLayer((activeTab == VendorWindowsTab.Trade) ? "Selected" : "Normal");
		m_ReputationViewButton.SetActiveLayer((activeTab == VendorWindowsTab.Reputation) ? "Selected" : "Normal");
		m_IsReputation.Value = CurrentTab.CurrentValue == VendorWindowsTab.Reputation;
	}

	public VendorWindowsTab GetActiveTab()
	{
		return base.ViewModel.ActiveTab.CurrentValue;
	}

	public void SetNextTab()
	{
		m_VendorViewButton.SetActiveLayer((base.ViewModel.ActiveTab.CurrentValue == VendorWindowsTab.Trade) ? "Normal" : "Selected");
		m_ReputationViewButton.SetActiveLayer((base.ViewModel.ActiveTab.CurrentValue == VendorWindowsTab.Reputation) ? "Normal" : "Selected");
		base.ViewModel.SetNextTab();
	}
}
