using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabDlcsBaseView : View<DlcManagerTabDlcsVM>
{
	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_DlcDescription;

	[SerializeField]
	private List<Image> m_DlcDescriptionBackgroundGradients;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Art")]
	[SerializeField]
	private Image m_DlcArt;

	[Header("Bottom Block")]
	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveThisDlc;

	[SerializeField]
	protected OwlcatButton m_PurchaseButton;

	[SerializeField]
	private TextMeshProUGUI m_PurchaseLabel;

	[SerializeField]
	private TextMeshProUGUI m_PurchasedLabel;

	[SerializeField]
	private TextMeshProUGUI m_ComingSoonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DownloadingInProgressText;

	[SerializeField]
	protected OwlcatButton m_InstallButton;

	[SerializeField]
	private TextMeshProUGUI m_InstallButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcIsBoughtAndNotInstalledText;

	[Header("Dlcs Block")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRectDlcs;

	protected bool IsInit;

	public virtual void Initialize()
	{
	}

	protected override void OnBind()
	{
		ScrollToTop();
		base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			base.ViewModel.ResetVideo();
			if (value)
			{
				ScrollToTop();
				UpdateDlcEntities();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ChangeStory, delegate
		{
			bool active = base.ViewModel.Art.CurrentValue != null && base.ViewModel.Video?.CurrentValue == null;
			m_DlcArt.gameObject.SetActive(active);
			if (base.ViewModel.Art.CurrentValue != null)
			{
				m_DlcArt.sprite = base.ViewModel.Art.CurrentValue;
			}
			VideoClip videoClip = base.ViewModel.Video?.CurrentValue;
			ShowHideVideo(videoClip != null);
			base.ViewModel.CustomUIVideoPlayerVM.SetVideo(videoClip, base.ViewModel.Art.CurrentValue, base.ViewModel.SoundStart.CurrentValue, base.ViewModel.SoundStop.CurrentValue);
			m_DlcDescription.text = base.ViewModel.Description.CurrentValue;
			m_ScrollRect.Or(null)?.ScrollToTop();
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
		}).AddTo(this);
		(from value in m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable()
			select 1f - value).Subscribe(delegate(float invertedValue)
		{
			foreach (Image dlcDescriptionBackgroundGradient in m_DlcDescriptionBackgroundGradients)
			{
				Color color = dlcDescriptionBackgroundGradient.color;
				color.a = ((!m_ScrollRect.verticalScrollbar.gameObject.activeSelf) ? 0f : invertedValue);
				dlcDescriptionBackgroundGradient.color = color;
			}
		}).AddTo(this);
		base.ViewModel.DownloadingInProgress.CombineLatest(base.ViewModel.DlcIsBoughtAndNotInstalled, (bool downloadInProgress, bool boughtAndNotInstalled) => new { downloadInProgress, boughtAndNotInstalled }).Subscribe(value =>
		{
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
			CheckInstallState(value.downloadInProgress, value.boughtAndNotInstalled);
		}).AddTo(this);
		CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.CurrentValue, base.ViewModel.DlcIsBought.CurrentValue);
		CheckInstallState(base.ViewModel.DownloadingInProgress.CurrentValue, base.ViewModel.DlcIsBoughtAndNotInstalled.CurrentValue);
		m_YouDontHaveThisDlc.text = UIStrings.Instance.DlcManager.AvailableForPurchase;
		m_PurchaseLabel.text = UIStrings.Instance.DlcManager.Purchase;
		m_PurchasedLabel.text = UIStrings.Instance.DlcManager.Purchased;
		m_ComingSoonLabel.text = UIStrings.Instance.DlcManager.ComingSoon;
		m_DownloadingInProgressText.text = UIStrings.Instance.DlcManager.DlcDownloading;
		m_DlcIsBoughtAndNotInstalledText.text = UIStrings.Instance.DlcManager.DlcBoughtAndNotInstalled;
		m_InstallButtonLabel.text = UIStrings.Instance.DlcManager.Install;
	}

	private void ShowHideVideo(bool state)
	{
		ShowHideVideoImpl(state);
	}

	protected virtual void ShowHideVideoImpl(bool state)
	{
	}

	private void UpdateDlcEntities()
	{
		UpdateDlcEntitiesImpl();
	}

	protected virtual void UpdateDlcEntitiesImpl()
	{
	}

	private void CheckAvailableState(bool canPurchase, bool available)
	{
		if (base.ViewModel.DownloadingInProgress.CurrentValue || base.ViewModel.DlcIsBoughtAndNotInstalled.CurrentValue)
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(value: false);
			m_PurchaseButton.gameObject.SetActive(value: false);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(value: false);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(!available && canPurchase);
			m_PurchaseButton.gameObject.SetActive(!available && canPurchase);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(available && canPurchase);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(!available && !canPurchase);
			CheckAvailableStateImpl(canPurchase, available);
		}
	}

	protected virtual void CheckAvailableStateImpl(bool canPurchase, bool available)
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
		m_ScrollRectDlcs.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectDlcs.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
