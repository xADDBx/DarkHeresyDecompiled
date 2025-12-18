using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationPartView<TVendorReputationForItem> : View<VendorReputationPartVM> where TVendorReputationForItem : VendorReputationForItemWindowView
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_DemandCargo;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationValues;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationHeader;

	[SerializeField]
	protected TextMeshProUGUI m_VendorReputationLevelInCircle;

	[SerializeField]
	protected TextMeshProUGUI FractionName;

	[SerializeField]
	protected TVendorReputationForItem m_ReputationForItemWindowPCView;

	[SerializeField]
	protected Image m_VendorReputationProgressToNextLevel;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private TextMeshProUGUI m_ExchangeValue;

	[SerializeField]
	protected OwlcatToggle m_ShowUnrelevantToggle;

	[SerializeField]
	private TextMeshProUGUI m_ShowUnrelevantLabel;

	[SerializeField]
	protected OwlcatButton SellButton;

	[SerializeField]
	protected TextMeshProUGUI m_SellButtonText;

	[SerializeField]
	protected CanvasGroup m_VendorInfoGroup;

	[SerializeField]
	protected CanvasGroup m_VendorHidenReputationGroup;

	[SerializeField]
	protected TextMeshProUGUI m_VendorHidenInfoText;

	private IDisposable m_ToggleDisposable;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_ReputationForItemWindowPCView.Initialize();
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
		SetReputation(base.ViewModel.NeedHidePfAndReputation);
		if (base.ViewModel.NeedHidePfAndReputation)
		{
			m_VendorHidenInfoText.text = UIStrings.Instance.QuesJournalTexts.NoData.Text;
		}
		m_ReputationForItemWindowPCView.Bind(base.ViewModel.VendorReputationForItemWindow);
		m_SelectorView.Bind(base.ViewModel.Selector);
		base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate
		{
			m_VendorReputationProgressToNextLevel.fillAmount = base.ViewModel.Difference.CurrentValue / (float)base.ViewModel.Delta.CurrentValue;
		}).AddTo(this);
		base.ViewModel.VendorReputationProgressToNextLevel.Subscribe(delegate(int? exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.CurrentValue ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : $"{base.ViewModel.VendorCurrentReputationProgress} / {exp.ToString()}");
		}).AddTo(this);
		base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate(float exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.CurrentValue ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : (exp + " / " + base.ViewModel.VendorReputationProgressToNextLevel.CurrentValue));
		}).AddTo(this);
		base.ViewModel.VendorReputationLevel.Subscribe(delegate(int l)
		{
			m_VendorReputationLevelInCircle.text = l.ToString();
		}).AddTo(this);
		base.ViewModel.ExchangeValue.Subscribe(delegate(int val)
		{
			m_ExchangeValue.text = val.ToString();
		}).AddTo(this);
		m_ReputationHeader.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		FractionName.text = base.ViewModel.VendorFractionName;
		m_ShowUnrelevantLabel.text = UIStrings.Instance.Vendor.HideUnrelevant;
		m_SellButtonText.text = UIStrings.Instance.Vendor.Exchange;
		base.ViewModel.CanSellCargo.Subscribe(SellButton.SetInteractable).AddTo(this);
		m_ToggleDisposable = m_ShowUnrelevantToggle.IsOn.Skip(1).Subscribe(delegate
		{
			base.ViewModel.HideUnrelevant();
		});
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
		m_ToggleDisposable?.Dispose();
		m_ToggleDisposable = null;
		base.gameObject.SetActive(value: false);
	}

	private void SetReputation(bool value)
	{
		m_VendorInfoGroup.gameObject.SetActive(!value);
		m_VendorHidenReputationGroup.gameObject.SetActive(value);
	}
}
