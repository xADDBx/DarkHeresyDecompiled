using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryVM : NewGamePhaseBaseVm, INewGameChangeDlcHandler, ISubscriber, INewGameSwitchOnOffDlcHandler
{
	private readonly ReactiveProperty<Sprite> m_Art = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<VideoClip> m_Video = new ReactiveProperty<VideoClip>();

	private readonly ReactiveProperty<string> m_SoundStart = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SoundStop = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_CampaignName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<NewGamePhaseStoryScenarioEntityVM> m_SelectedEntity = new ReactiveProperty<NewGamePhaseStoryScenarioEntityVM>();

	private BlueprintCampaignReference m_CurrentCampaignReference;

	public readonly SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM> SelectionGroup;

	private readonly ReactiveCommand<Unit> m_ChangeStory = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_DlcIsBought = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DlcIsAvailableToPurchase = new ReactiveProperty<bool>();

	public BlueprintDlc BlueprintDlc;

	private readonly ReactiveProperty<bool> m_DlcIsOn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DownloadingInProgress = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DlcIsBoughtAndNotInstalled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsRealConsole = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> IsDlcRequired { get; } = new ReactiveProperty<bool>();


	public ReadOnlyReactiveProperty<Sprite> Art => m_Art;

	public ReadOnlyReactiveProperty<VideoClip> Video => m_Video;

	public ReadOnlyReactiveProperty<string> SoundStart => m_SoundStart;

	public ReadOnlyReactiveProperty<string> SoundStop => m_SoundStop;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public ReadOnlyReactiveProperty<string> CampaignName => m_CampaignName;

	private BlueprintCampaign CurrentCampaign
	{
		get
		{
			if (m_CurrentCampaignReference != null && !m_CurrentCampaignReference.IsEmpty())
			{
				return m_CurrentCampaignReference.Get();
			}
			return null;
		}
		set
		{
			m_CurrentCampaignReference = value?.ToReference<BlueprintCampaignReference>();
		}
	}

	public Observable<Unit> ChangeStory => m_ChangeStory;

	public ReadOnlyReactiveProperty<bool> DlcIsBought => m_DlcIsBought;

	public ReadOnlyReactiveProperty<bool> DlcIsAvailableToPurchase => m_DlcIsAvailableToPurchase;

	public ReadOnlyReactiveProperty<bool> DlcIsOn => m_DlcIsOn;

	public ReadOnlyReactiveProperty<bool> DownloadingInProgress => m_DownloadingInProgress;

	public ReadOnlyReactiveProperty<bool> DlcIsBoughtAndNotInstalled => m_DlcIsBoughtAndNotInstalled;

	public ReadOnlyReactiveProperty<bool> IsRealConsole => m_IsRealConsole;

	public CustomUIVideoPlayerVM CustomUIVideoPlayerVM { get; }

	public NewGamePhaseStoryVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		List<NewGamePhaseStoryScenarioEntityVM> list = new List<NewGamePhaseStoryScenarioEntityVM>();
		foreach (BlueprintCampaign campaign in ConfigRoot.Instance.NewGameSettings.StoryCampaigns)
		{
			NewGamePhaseStoryScenarioEntityVM newGamePhaseStoryScenarioEntityVM = new NewGamePhaseStoryScenarioEntityVM(campaign, delegate
			{
				SetStory(campaign);
			});
			list.Add(newGamePhaseStoryScenarioEntityVM);
			newGamePhaseStoryScenarioEntityVM.AddTo(this);
		}
		SelectionGroup = new SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM>(list, m_SelectedEntity);
		SelectionGroup.AddTo(this);
		m_SelectedEntity.Value = list.First();
		CustomUIVideoPlayerVM = new CustomUIVideoPlayerVM().AddTo(this);
		m_IsRealConsole.Value = false;
		EventBus.Subscribe(this).AddTo(this);
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void OnDispose()
	{
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
		base.OnDispose();
	}

	private void HandleOnRefreshDLC()
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(BlueprintDlc);
		m_DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && IsRealConsole.CurrentValue;
		m_DlcIsBought.Value = iDLCStatus.Purchased;
		m_DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && IsRealConsole.CurrentValue;
		SetStory(CurrentCampaign, BlueprintDlc);
	}

	private void SetStory(BlueprintCampaign campaign, BlueprintDlc blueprintDlc = null)
	{
		m_CampaignName.Value = campaign.Title;
		m_Video.Value = ((blueprintDlc != null) ? blueprintDlc.DefaultVideo : campaign.Video);
		m_SoundStart.Value = ((blueprintDlc != null) ? blueprintDlc.SoundStartEvent : campaign.SoundStartEvent);
		m_SoundStop.Value = ((blueprintDlc != null) ? blueprintDlc.SoundStopEvent : campaign.SoundStopEvent);
		m_Art.Value = ((blueprintDlc?.DefaultKeyArt != null) ? blueprintDlc.DefaultKeyArt : campaign.KeyArt);
		m_Description.Value = ((blueprintDlc != null) ? blueprintDlc.DlcDescription : ((string)campaign.Description));
		BlueprintDlc = blueprintDlc;
		BlueprintAreaPreset blueprintAreaPreset = campaign.StartGamePreset ?? GameStarter.MainPreset;
		bool flag = campaign.IsAvailable && blueprintAreaPreset != null;
		m_DlcIsBought.Value = blueprintDlc?.IsPurchased ?? flag;
		m_DlcIsAvailableToPurchase.Value = blueprintDlc == null || blueprintDlc.GetPurchaseState() != BlueprintDlc.DlcPurchaseState.ComingSoon;
		m_DlcIsOn.Value = blueprintDlc?.GetDlcSwitchOnOffState() ?? false;
		m_DownloadingInProgress.Value = blueprintDlc != null && blueprintDlc.GetDownloadState() == DownloadState.Loading && IsRealConsole.CurrentValue;
		m_DlcIsBoughtAndNotInstalled.Value = DlcIsBought.CurrentValue && blueprintDlc != null && blueprintDlc.GetDownloadState() == DownloadState.NotLoaded && IsRealConsole.CurrentValue;
		if (CurrentCampaign == campaign)
		{
			m_ChangeStory.Execute(Unit.Default);
			return;
		}
		MainMenuChargenUnits.Instance.DlcReward = campaign.DlcReward;
		IsDlcRequired.Value = !flag;
		SetNextButtonAvailable(flag);
		CurrentCampaign = campaign;
		m_ChangeStory.Execute(Unit.Default);
	}

	public void ShowInStore()
	{
		if (BlueprintDlc != null)
		{
			PFLog.UI.Log($"Open {BlueprintDlc} store");
			StoreManager.OpenShopFor(BlueprintDlc);
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxType.Message, null);
			return;
		}
		BlueprintDlc storyBlueprintDlc = GetStoryBlueprintDlc(CurrentCampaign);
		if (storyBlueprintDlc == null)
		{
			PFLog.UI.Log($"Story dlc of {CurrentCampaign} is null");
			return;
		}
		PFLog.UI.Log($"Open {CurrentCampaign.DlcReward} store");
		StoreManager.OpenShopFor(storyBlueprintDlc);
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxType.Message, null);
	}

	private BlueprintDlc GetStoryBlueprintDlc(BlueprintCampaign campaign)
	{
		return StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>().FirstOrDefault((BlueprintDlc pd) => pd.Rewards.FirstOrDefault((IBlueprintDlcReward r) => r is BlueprintDlcRewardCampaign blueprintDlcRewardCampaign && blueprintDlcRewardCampaign.Campaign == campaign) != null);
	}

	public void SwitchDlcOn()
	{
		BlueprintDlc?.SwitchDlcValue(!DlcIsOn.CurrentValue);
	}

	public override void OnNext()
	{
		if (base.IsNextButtonAvailable.CurrentValue)
		{
			if (CurrentCampaign.AdditionalContentDlc.Any((BlueprintDlc dlc) => dlc.GetDownloadState() == DownloadState.Loading) || DownloadingInProgress.CurrentValue)
			{
				UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.NeedWaitAllDlcsDownload, DialogMessageBoxType.Message, null);
				return;
			}
			Game.NewGamePreset = GetPreset();
			base.OnNext();
		}
	}

	private BlueprintAreaPreset GetPreset()
	{
		return MainMenuChargenUnits.Instance.DlcReward?.Campaign?.StartGamePreset ?? GameStarter.MainPreset;
	}

	public void InstallDlc()
	{
		if (BlueprintDlc != null)
		{
			m_DownloadingInProgress.Value = true;
			m_DlcIsBoughtAndNotInstalled.Value = false;
			StoreManager.InstallDlc(BlueprintDlc);
		}
	}

	public void DeleteDlc()
	{
		if (BlueprintDlc == null)
		{
			return;
		}
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.AreYouSureDeleteDlc, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				m_DownloadingInProgress.Value = true;
				BlueprintDlc?.SwitchDlcValue(value: false);
				StoreManager.DeleteDlc(BlueprintDlc);
			}
		});
	}

	public void ResetVideo()
	{
		CustomUIVideoPlayerVM.ResetVideo();
	}

	public void HandleNewGameChangeDlc(BlueprintCampaign campaign, BlueprintDlc blueprintDlc)
	{
		SetStory(campaign, blueprintDlc);
	}

	public void HandleNewGameSwitchOnOffDlc(BlueprintDlc dlc, bool value)
	{
		if (dlc == BlueprintDlc)
		{
			m_DlcIsOn.Value = value;
		}
	}
}
