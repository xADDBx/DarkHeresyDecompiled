using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryBaseView : View<NewGamePhaseStoryVM>
{
	[Header("Common")]
	[SerializeField]
	private PantographView m_PantographView;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_StoryDescription;

	[SerializeField]
	private List<Image> m_StoryDescriptionBackgroundGradients;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Art")]
	[SerializeField]
	private Image m_StoryArt;

	[Header("Bottom Block")]
	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveThisDlc;

	[SerializeField]
	private TextMeshProUGUI m_DlcStatusLabel;

	[SerializeField]
	protected OwlcatButton m_PurchaseButton;

	[SerializeField]
	private TextMeshProUGUI m_PurchaseLabel;

	[SerializeField]
	private TextMeshProUGUI m_PurchasedLabel;

	[SerializeField]
	private TextMeshProUGUI m_ComingSoonLabel;

	[SerializeField]
	protected OwlcatMultiButton m_SwitchOnDlcButton;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	[SerializeField]
	private TextMeshProUGUI m_DownloadingInProgressText;

	[SerializeField]
	protected OwlcatButton m_InstallButton;

	[SerializeField]
	private TextMeshProUGUI m_InstallButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcIsBoughtAndNotInstalledText;

	protected readonly ReactiveProperty<bool> SwitchOnButtonActive = new ReactiveProperty<bool>();

	protected bool IsInit;

	public virtual void Initialize()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				base.ViewModel.ResetVideo();
				ScrollToTop();
				m_PantographView.Show();
			}
			else
			{
				m_PantographView.Hide();
			}
		}).AddTo(this);
		m_PantographView.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ChangeStory, delegate
		{
			bool active = base.ViewModel.Art.CurrentValue != null && base.ViewModel.Video?.CurrentValue == null;
			m_StoryArt.gameObject.SetActive(active);
			if (base.ViewModel.Art.CurrentValue != null)
			{
				m_StoryArt.sprite = base.ViewModel.Art.CurrentValue;
			}
			VideoClip videoClip = base.ViewModel.Video?.CurrentValue;
			ShowHideVideo(videoClip != null);
			base.ViewModel.CustomUIVideoPlayerVM.SetVideo(videoClip, base.ViewModel.Art.CurrentValue, base.ViewModel.SoundStart.CurrentValue, base.ViewModel.SoundStop.CurrentValue);
			m_StoryDescription.text = base.ViewModel.Description.CurrentValue;
			ScrollToTop();
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
		}).AddTo(this);
		base.ViewModel.DlcIsOn.Subscribe(delegate(bool value)
		{
			m_SwitchOnDlcButton.SetActiveLayer(value ? "On" : "Off");
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SwitchOnDlcButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SwitchDlcOn();
		}).AddTo(this);
		m_DlcStatusLabel.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.DlcManager.DlcSwitchOnOffHint)).AddTo(this);
		(from value in m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable()
			select 1f - value).Subscribe(delegate(float invertedValue)
		{
			foreach (Image storyDescriptionBackgroundGradient in m_StoryDescriptionBackgroundGradients)
			{
				Color color = storyDescriptionBackgroundGradient.color;
				color.a = ((!m_ScrollRect.verticalScrollbar.gameObject.activeSelf) ? 0f : invertedValue);
				storyDescriptionBackgroundGradient.color = color;
			}
		}).AddTo(this);
		base.ViewModel.DownloadingInProgress.CombineLatest(base.ViewModel.DlcIsBoughtAndNotInstalled, (bool downloadInProgress, bool boughtAndNotInstalled) => new { downloadInProgress, boughtAndNotInstalled }).Subscribe(value =>
		{
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
			CheckInstallState(value.downloadInProgress, value.boughtAndNotInstalled);
		}).AddTo(this);
		CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
		CheckInstallState(base.ViewModel.DownloadingInProgress.CurrentValue, base.ViewModel.DlcIsBoughtAndNotInstalled.CurrentValue);
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
		m_DlcStatusLabel.text = UIStrings.Instance.DlcManager.DlcStatus;
		m_PurchaseLabel.text = UIStrings.Instance.DlcManager.Purchase;
		m_PurchasedLabel.text = UIStrings.Instance.DlcManager.Purchased;
		m_ComingSoonLabel.text = UIStrings.Instance.DlcManager.ComingSoon;
		m_DownloadingInProgressText.text = UIStrings.Instance.DlcManager.DlcDownloading;
		m_DlcIsBoughtAndNotInstalledText.text = UIStrings.Instance.DlcManager.DlcBoughtAndNotInstalled;
		m_InstallButtonLabel.text = UIStrings.Instance.DlcManager.Install;
	}

	protected override void OnUnbind()
	{
	}

	private void ShowHideVideo(bool state)
	{
		ShowHideVideoImpl(state);
	}

	protected virtual void ShowHideVideoImpl(bool state)
	{
	}

	private void CheckAvailableState(bool canPurchase, bool available)
	{
		if (base.ViewModel.DownloadingInProgress.CurrentValue || base.ViewModel.DlcIsBoughtAndNotInstalled.CurrentValue)
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(value: false);
			SetPurchaseButtonVisible(visible: false);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(value: false);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(value: false);
			m_SwitchOnDlcButton.transform.parent.gameObject.SetActive(value: false);
			SwitchOnButtonActive.Value = false;
		}
		else
		{
			UIDlcManager dlcManager = UIStrings.Instance.DlcManager;
			m_YouDontHaveThisDlc.text = ((base.ViewModel.BlueprintDlc == null) ? ((!base.ViewModel.IsNextButtonAvailable.CurrentValue) ? ((string)dlcManager.YouDontHaveThisDlc) : string.Empty) : ((!available && canPurchase) ? ((string)dlcManager.YouDontHaveThisDlc) : ((!base.ViewModel.IsNextButtonAvailable.CurrentValue) ? string.Format(dlcManager.YouDontHaveThisStory, base.ViewModel.CampaignName.CurrentValue) : string.Empty)));
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive((!available && canPurchase) || !base.ViewModel.IsNextButtonAvailable.CurrentValue);
			SetPurchaseButtonVisible(!available && canPurchase);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(available && canPurchase);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(!available && !canPurchase);
			CheckAvailableStateImpl(available);
			SwitchOnButtonActive.Value = available && base.ViewModel.BlueprintDlc != null && base.ViewModel.IsNextButtonAvailable.CurrentValue;
			m_SwitchOnDlcButton.transform.parent.gameObject.SetActive(SwitchOnButtonActive.Value);
		}
	}

	protected virtual void CheckAvailableStateImpl(bool available)
	{
	}

	private void CheckInstallState(bool downloadingInProgress, bool boughtAndNotInstalled)
	{
		m_DownloadingInProgressText.transform.parent.gameObject.SetActive(downloadingInProgress && !boughtAndNotInstalled);
		m_DlcIsBoughtAndNotInstalledText.transform.parent.gameObject.SetActive(!downloadingInProgress && boughtAndNotInstalled);
		m_InstallButton.gameObject.SetActive(!downloadingInProgress && boughtAndNotInstalled);
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
	}

	private void SetPurchaseButtonVisible(bool visible)
	{
		m_PurchaseButton.gameObject.SetActive(visible);
	}
}
