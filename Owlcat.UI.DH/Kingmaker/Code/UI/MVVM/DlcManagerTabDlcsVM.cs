using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabDlcsVM : DlcManagerTabBaseVM
{
	private readonly ReactiveProperty<Sprite> m_Art = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<VideoClip> m_Video = new ReactiveProperty<VideoClip>();

	private readonly ReactiveProperty<string> m_SoundStart = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SoundStop = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	private readonly ReactiveProperty<DlcManagerDlcEntityVM> m_SelectedEntity = new ReactiveProperty<DlcManagerDlcEntityVM>();

	public readonly SelectionGroupRadioVM<DlcManagerDlcEntityVM> SelectionGroup;

	private BlueprintDlc m_CurrentDlc;

	private readonly ReactiveCommand<Unit> m_ChangeStory = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_DlcIsBought = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DlcIsAvailableToPurchase = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DownloadingInProgress = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DlcIsBoughtAndNotInstalled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsRealConsole = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<DlcManagerDlcEntityVM> SelectedEntity => m_SelectedEntity;

	public Observable<Unit> ChangeStory => m_ChangeStory;

	public ReadOnlyReactiveProperty<bool> DlcIsBought => m_DlcIsBought;

	public ReadOnlyReactiveProperty<bool> DlcIsAvailableToPurchase => m_DlcIsAvailableToPurchase;

	public ReadOnlyReactiveProperty<bool> DownloadingInProgress => m_DownloadingInProgress;

	public ReadOnlyReactiveProperty<bool> DlcIsBoughtAndNotInstalled => m_DlcIsBoughtAndNotInstalled;

	public ReadOnlyReactiveProperty<bool> IsRealConsole => m_IsRealConsole;

	public ReadOnlyReactiveProperty<Sprite> Art => m_Art;

	public ReadOnlyReactiveProperty<VideoClip> Video => m_Video;

	public ReadOnlyReactiveProperty<string> SoundStart => m_SoundStart;

	public ReadOnlyReactiveProperty<string> SoundStop => m_SoundStop;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public CustomUIVideoPlayerVM CustomUIVideoPlayerVM { get; }

	public DlcManagerTabDlcsVM()
	{
		List<DlcManagerDlcEntityVM> list = new List<DlcManagerDlcEntityVM>();
		foreach (IBlueprintDlc purchasableDLC in StoreManager.GetPurchasableDLCs())
		{
			BlueprintDlc dlc = purchasableDLC as BlueprintDlc;
			if (dlc == null || !dlc.HideDlc)
			{
				DlcManagerDlcEntityVM item = new DlcManagerDlcEntityVM(dlc, delegate
				{
					SetDlc(dlc);
				}).AddTo(this);
				list.Add(item);
			}
		}
		List<DlcManagerDlcEntityVM> list2 = list.OrderBy((DlcManagerDlcEntityVM dlc) => dlc.DlcType).ToList();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerDlcEntityVM>(list2, m_SelectedEntity).AddTo(this);
		m_SelectedEntity.Value = list2.FirstOrDefault();
		CustomUIVideoPlayerVM = new CustomUIVideoPlayerVM().AddTo(this);
		m_IsRealConsole.Value = false;
		EventBus.Subscribe(this).AddTo(this);
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void OnDispose()
	{
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
	}

	private void HandleOnRefreshDLC()
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(m_CurrentDlc);
		m_DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && IsRealConsole.CurrentValue;
		m_DlcIsBought.Value = iDLCStatus.Purchased;
		m_DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && IsRealConsole.CurrentValue;
		SetDlc(m_CurrentDlc);
	}

	private void SetDlc(BlueprintDlc blueprintDlc)
	{
		if (blueprintDlc != null)
		{
			m_CurrentDlc = blueprintDlc;
			m_Video.Value = blueprintDlc.DefaultVideo;
			m_SoundStart.Value = blueprintDlc.SoundStartEvent;
			m_SoundStop.Value = blueprintDlc.SoundStopEvent;
			m_Art.Value = ((blueprintDlc.DefaultKeyArt != null) ? blueprintDlc.DefaultKeyArt : UIConfig.Instance.KeyArt);
			m_Description.Value = blueprintDlc.DlcDescription;
			m_DlcIsBought.Value = blueprintDlc.IsPurchased;
			m_DlcIsAvailableToPurchase.Value = blueprintDlc.GetPurchaseState() != BlueprintDlc.DlcPurchaseState.ComingSoon;
			m_DownloadingInProgress.Value = blueprintDlc.GetDownloadState() == DownloadState.Loading && IsRealConsole.CurrentValue;
			m_DlcIsBoughtAndNotInstalled.Value = blueprintDlc.IsPurchased && blueprintDlc.GetDownloadState() == DownloadState.NotLoaded && IsRealConsole.CurrentValue;
			m_ChangeStory.Execute();
		}
	}

	public void ShowInStore()
	{
		if (m_CurrentDlc != null)
		{
			PFLog.UI.Log($"Open {m_CurrentDlc} store");
			StoreManager.OpenShopFor(m_CurrentDlc);
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxType.Message, null);
		}
	}

	public void InstallDlc()
	{
		if (m_CurrentDlc != null)
		{
			m_DownloadingInProgress.Value = true;
			m_DlcIsBoughtAndNotInstalled.Value = false;
			if (SelectedEntity?.CurrentValue != null)
			{
				m_DownloadingInProgress.Value = true;
				m_DlcIsBoughtAndNotInstalled.Value = false;
			}
			StoreManager.InstallDlc(m_CurrentDlc);
		}
	}

	public void DeleteDlc()
	{
		if (m_CurrentDlc == null)
		{
			return;
		}
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.AreYouSureDeleteDlc, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				m_DownloadingInProgress.Value = true;
				if (SelectedEntity?.CurrentValue != null)
				{
					m_DownloadingInProgress.Value = true;
				}
				StoreManager.DeleteDlc(m_CurrentDlc);
			}
		});
	}

	public void ResetVideo()
	{
		CustomUIVideoPlayerVM.ResetVideo();
	}

	public void SetSelectedEntityVM(DlcManagerDlcEntityVM entityVM)
	{
		m_SelectedEntity.Value = entityVM;
		m_SelectedEntity.ForceNotify();
	}
}
