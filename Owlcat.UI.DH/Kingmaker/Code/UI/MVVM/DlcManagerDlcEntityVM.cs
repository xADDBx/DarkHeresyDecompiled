using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerDlcEntityVM : SelectionGroupEntityVM
{
	public readonly BlueprintDlc BlueprintDlc;

	private Action m_SetDlc;

	public readonly string Title;

	public readonly Sprite Art;

	public readonly DlcTypeEnum DlcType;

	private readonly string ICH_HABE_ES_GESEHEN_PREF_KEY;

	private readonly ReactiveProperty<bool> m_SawThisDlc = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DownloadingInProgress = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DlcIsBoughtAndNotInstalled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsDlcCanBeDeleted = new ReactiveProperty<bool>();

	private readonly bool m_IsRealConsole;

	public ReadOnlyReactiveProperty<bool> SawThisDlc => m_SawThisDlc;

	public ReadOnlyReactiveProperty<bool> DownloadingInProgress => m_DownloadingInProgress;

	public ReadOnlyReactiveProperty<bool> DlcIsBoughtAndNotInstalled => m_DlcIsBoughtAndNotInstalled;

	public ReadOnlyReactiveProperty<bool> IsDlcCanBeDeleted => m_IsDlcCanBeDeleted;

	private bool CurrentSawValue => PlayerPrefs.GetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 0) == 1;

	public DlcManagerDlcEntityVM(BlueprintDlc blueprintDlc, Action setDlc)
		: base(allowSwitchOff: false)
	{
		m_IsRealConsole = false;
		BlueprintDlc = blueprintDlc;
		m_SetDlc = setDlc;
		Title = blueprintDlc.GetDlcName();
		Art = ((blueprintDlc.DlcItemArtLink != null) ? blueprintDlc.DlcItemArtLink : UIConfig.Instance.DlcEntityKeyArt);
		DlcType = blueprintDlc.DlcType;
		DownloadState downloadState = blueprintDlc.GetDownloadState();
		m_DownloadingInProgress.Value = downloadState == DownloadState.Loading && m_IsRealConsole;
		m_DlcIsBoughtAndNotInstalled.Value = blueprintDlc.IsPurchased && downloadState == DownloadState.NotLoaded && m_IsRealConsole;
		m_IsDlcCanBeDeleted.Value = downloadState == DownloadState.Loaded && blueprintDlc.DlcType == DlcTypeEnum.AdditionalContentDlc;
		ICH_HABE_ES_GESEHEN_PREF_KEY = "DLCMANAGER_I_SAW_" + blueprintDlc.name;
		m_SawThisDlc.Value = CurrentSawValue;
		AddDisposable(EventBus.Subscribe(this));
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
		m_SetDlc = null;
	}

	private void HandleOnRefreshDLC()
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(BlueprintDlc);
		m_DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && m_IsRealConsole;
		m_DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && m_IsRealConsole;
		m_IsDlcCanBeDeleted.Value = iDLCStatus.DownloadState == DownloadState.Loaded && BlueprintDlc.DlcType == DlcTypeEnum.AdditionalContentDlc;
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_SetDlc?.Invoke();
		if (!CurrentSawValue)
		{
			PlayerPrefs.SetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 1);
			PlayerPrefs.Save();
			if (SawThisDlc.CurrentValue != CurrentSawValue)
			{
				m_SawThisDlc.Value = CurrentSawValue;
			}
		}
	}
}
