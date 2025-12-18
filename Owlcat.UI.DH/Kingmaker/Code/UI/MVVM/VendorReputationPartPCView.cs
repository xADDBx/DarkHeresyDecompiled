using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationPartPCView : VendorReputationPartView<VendorReputationForItemWindowPCView>
{
	[SerializeField]
	protected OwlcatButton m_SelectAllButton;

	[SerializeField]
	protected TextMeshProUGUI m_SelectAllButtonText;

	[SerializeField]
	protected OwlcatButton m_UnselectAllButton;

	[SerializeField]
	protected TextMeshProUGUI m_UnselectButtonText;

	[SerializeField]
	private OwlcatMultiButton m_CargoButton;

	[SerializeField]
	private OwlcatMultiButton m_ListButton;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.HasItemsToSell.Subscribe(m_SelectAllButton.SetInteractable).AddTo(this);
		base.ViewModel.CanSellCargo.Subscribe(m_UnselectAllButton.SetInteractable).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(SellButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SellCargo();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SelectAllButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SelectAll();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_UnselectAllButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.UnselectAll();
		}).AddTo(this);
		m_SelectAllButton.SetHint(UIStrings.Instance.Vendor.SelectAllRelevant).AddTo(this);
		m_UnselectAllButton.SetHint(UIStrings.Instance.Vendor.UnselectAllRelevant).AddTo(this);
		base.ViewModel.CanSellCargo.Subscribe(delegate(bool val)
		{
			SellButton.Interactable = val;
		}).AddTo(this);
		m_SelectAllButtonText.text = UIStrings.Instance.Vendor.SelectAllRelevant;
		m_UnselectButtonText.text = UIStrings.Instance.Vendor.UnselectAllRelevant;
	}
}
