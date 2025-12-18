using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.UIUtils;
using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Save;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Fsm;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyVM : ViewModel, INetEvents, ISubscriber, ISavesUpdatedHandler, IAreaHandler, INetLobbyPlayersHandler, INetSaveSelectHandler, INetLobbyEpicGamesEvents, INetCheckUsersModsHandler, INetLobbyRequest, StateMachine<NetGame.State, NetGame.Trigger>.IStateMachineEventsHandler
{
	private const string NET_LOBBY_TUTORIAL_PREF_KEY = "first_open_net_lobby_tutorial";

	private const string NET_LOBBY_LAST_SELECTED_JOINABLE_USER_PREF_KEY = "net_lobby_selected_joinable_user";

	private const string NET_LOBBY_LAST_SELECTED_INVITABLE_USER_PREF_KEY = "net_lobby_selected_invitable_user";

	private readonly Action m_CloseAction;

	private string m_Code = string.Empty;

	private readonly ReactiveProperty<bool> m_HasCodeForLobby = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<OwlcatDropdownVM> m_RegionDropdownVM = new ReactiveProperty<OwlcatDropdownVM>(null);

	private readonly ReactiveProperty<bool> m_IsHost = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsInRoom = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_ReadyToHostOrJoin = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSaveAllowed = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSaveTransfer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_SaveTransferProgress = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_SaveTransferTarget = new ReactiveProperty<int>(1);

	private readonly ReactiveProperty<string> m_CurrentRegion = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_LobbyCode = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_Version = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<NetGame.State> m_NetGameCurrentState = new ReactiveProperty<NetGame.State>(NetGame.State.PlatformNotInitialized);

	private readonly ReactiveProperty<SaveSlotVM> m_CurrentSave = new ReactiveProperty<SaveSlotVM>(null);

	private readonly ReactiveProperty<bool> m_NeedReconnect = new ReactiveProperty<bool>(value: false);

	public readonly List<NetLobbyPlayerVM> PlayerVms = new List<NetLobbyPlayerVM>();

	private readonly ReactiveProperty<SaveSlotCollectionVM> m_SaveSlotCollectionVm = new ReactiveProperty<SaveSlotCollectionVM>();

	private readonly ReactiveProperty<bool> m_ShowWaitingSaveAnim = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_SaveListAreEmpty = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_CanConfirmLaunch = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<SaveSlotVM> m_SaveFullScreenshot = new ReactiveProperty<SaveSlotVM>();

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	private bool m_NeedUpdateRegion = true;

	private readonly ReactiveProperty<NetLobbyTutorialPartVM> m_NetLobbyTutorialPartVM = new ReactiveProperty<NetLobbyTutorialPartVM>();

	private readonly ReactiveProperty<NetLobbyDlcListVM> m_DlcListVM = new ReactiveProperty<NetLobbyDlcListVM>();

	private readonly Dictionary<string, List<IBlueprintDlc>> m_DifferentDlcWithSaveProblems = new Dictionary<string, List<IBlueprintDlc>>();

	public readonly Dictionary<string, List<IBlueprintDlc>> ProblemsToShowInSaveList = new Dictionary<string, List<IBlueprintDlc>>();

	private readonly ReactiveCommand<Dictionary<string, List<IBlueprintDlc>>> m_CheckProblemsWithDlcs = new ReactiveCommand<Dictionary<string, List<IBlueprintDlc>>>();

	private readonly ReactiveProperty<bool> m_NetLobbyTutorialOnScreen = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsAnyTutorialBlocks = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsEnoughPlayersForGame = new ReactiveProperty<bool>();

	private NetLobbyErrorHandler m_NetLobbyErrorHandler;

	private readonly ReactiveProperty<bool> m_IsPlayingState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_EpicGamesButtonActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_EpicGamesAuthorized = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_EpicGamesUserName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> m_DifferentPlatformInviteVM = new ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM>();

	private readonly ReactiveProperty<string> m_PlayersDifferentMods = new ReactiveProperty<string>();

	public OwlcatDropdownVM JoinableUserTypesDropdownVM;

	public OwlcatDropdownVM InvitableUserTypesDropdownVM;

	private readonly ReactiveProperty<JoinableUserTypes> m_CurrentJoinableUserType = new ReactiveProperty<JoinableUserTypes>();

	private readonly ReactiveProperty<InvitableUserTypes> m_CurrentInvitableUserType = new ReactiveProperty<InvitableUserTypes>();

	private static bool NetLobbyTutorialHasShown => PlayerPrefs.GetInt("first_open_net_lobby_tutorial", 0) == 1;

	public ReadOnlyReactiveProperty<bool> HasCodeForLobby => m_HasCodeForLobby;

	public ReadOnlyReactiveProperty<OwlcatDropdownVM> RegionDropdownVM => m_RegionDropdownVM;

	public ReadOnlyReactiveProperty<bool> IsHost => m_IsHost;

	public ReadOnlyReactiveProperty<bool> IsInRoom => m_IsInRoom;

	public ReadOnlyReactiveProperty<bool> ReadyToHostOrJoin => m_ReadyToHostOrJoin;

	public ReadOnlyReactiveProperty<bool> IsSaveAllowed => m_IsSaveAllowed;

	public ReadOnlyReactiveProperty<bool> IsSaveTransfer => m_IsSaveTransfer;

	public ReadOnlyReactiveProperty<int> SaveTransferProgress => m_SaveTransferProgress;

	public ReadOnlyReactiveProperty<int> SaveTransferTarget => m_SaveTransferTarget;

	public ReadOnlyReactiveProperty<string> CurrentRegion => m_CurrentRegion;

	public ReadOnlyReactiveProperty<string> LobbyCode => m_LobbyCode;

	public ReadOnlyReactiveProperty<string> Version => m_Version;

	public ReadOnlyReactiveProperty<NetGame.State> NetGameCurrentState => m_NetGameCurrentState;

	public ReadOnlyReactiveProperty<SaveSlotVM> CurrentSave => m_CurrentSave;

	public ReadOnlyReactiveProperty<bool> NeedReconnect => m_NeedReconnect;

	public ReadOnlyReactiveProperty<SaveSlotCollectionVM> SaveSlotCollectionVm => m_SaveSlotCollectionVm;

	public ReadOnlyReactiveProperty<bool> ShowWaitingSaveAnim => m_ShowWaitingSaveAnim;

	public ReadOnlyReactiveProperty<bool> SaveListAreEmpty => m_SaveListAreEmpty;

	public ReadOnlyReactiveProperty<bool> CanConfirmLaunch => m_CanConfirmLaunch;

	public ReadOnlyReactiveProperty<SaveSlotVM> SaveFullScreenshot => m_SaveFullScreenshot;

	private ReactiveProperty<SaveLoadMode> SaveLoadMode { get; } = new ReactiveProperty<SaveLoadMode>(Kingmaker.Code.View.Bridge.Enums.SaveLoadMode.Load);


	public ReadOnlyReactiveProperty<NetLobbyTutorialPartVM> NetLobbyTutorialPartVM => m_NetLobbyTutorialPartVM;

	public ReadOnlyReactiveProperty<NetLobbyDlcListVM> DlcListVM => m_DlcListVM;

	public Observable<Dictionary<string, List<IBlueprintDlc>>> CheckProblemsWithDlcs => m_CheckProblemsWithDlcs;

	public ReadOnlyReactiveProperty<bool> NetLobbyTutorialOnScreen => m_NetLobbyTutorialOnScreen;

	public ReadOnlyReactiveProperty<bool> IsAnyTutorialBlocks => m_IsAnyTutorialBlocks;

	public ReadOnlyReactiveProperty<bool> IsEnoughPlayersForGame => m_IsEnoughPlayersForGame;

	public bool IsConnectingNetGameCurrentState
	{
		get
		{
			NetGame.State currentValue = NetGameCurrentState.CurrentValue;
			return currentValue == NetGame.State.PlatformInitializing || currentValue == NetGame.State.NetInitializing || currentValue == NetGame.State.ChangingRegion || currentValue == NetGame.State.CreatingLobby || currentValue == NetGame.State.JoiningLobby;
		}
	}

	public bool IsMainMenu => GameUIState.Instance.IsInMainMenuObservable.Value;

	public ReadOnlyReactiveProperty<bool> IsPlayingState => m_IsPlayingState;

	public ReadOnlyReactiveProperty<bool> EpicGamesButtonActive => m_EpicGamesButtonActive;

	public ReadOnlyReactiveProperty<bool> EpicGamesAuthorized => m_EpicGamesAuthorized;

	public ReadOnlyReactiveProperty<string> EpicGamesUserName => m_EpicGamesUserName;

	public ReadOnlyReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> DifferentPlatformInviteVM => m_DifferentPlatformInviteVM;

	public ReadOnlyReactiveProperty<string> PlayersDifferentMods => m_PlayersDifferentMods;

	public ReadOnlyReactiveProperty<JoinableUserTypes> CurrentJoinableUserType => m_CurrentJoinableUserType;

	public ReadOnlyReactiveProperty<InvitableUserTypes> CurrentInvitableUserType => m_CurrentInvitableUserType;

	public NetLobbyVM(Action closeAction)
	{
		EventBus.Subscribe(this).AddTo(this);
		ActivateNetHandlers();
		m_CloseAction = closeAction;
		m_IsAnyTutorialBlocks.Value = UIConfig.Instance.BlueprintUINetLobbyTutorial.TutorialBlocksInfo.Any();
		m_IsSaveAllowed.Value = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, IsMainMenu);
		PhotonManager.NetGame.StartNetGameIfNeeded();
		for (int i = 0; i < 6; i++)
		{
			PlayerVms.Add(new NetLobbyPlayerVM(m_DifferentPlatformInviteVM, m_EpicGamesAuthorized));
		}
		m_NetGameCurrentState.Value = PhotonManager.NetGame.CurrentState;
		m_IsPlayingState.Value = NetGameCurrentState.CurrentValue == NetGame.State.Playing;
		m_Version.Value = PhotonManager.Version;
		UpdateRoom();
		IsInRoom.Subscribe(delegate(bool value)
		{
			m_IsHost.Value = value && PhotonManager.Instance.IsRoomOwner;
			if (value)
			{
				NetRoomNameHelper.TryFormatString(PhotonManager.Instance.Region, PhotonManager.Instance.RoomName, out var output);
				m_LobbyCode.Value = output;
				m_CurrentRegion.Value = PhotonManager.Instance.Region;
				m_NeedReconnect.Value = PhotonManager.Sync.HasDesync;
				m_IsSaveAllowed.Value = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, IsMainMenu);
				if (!PhotonManager.Instance.IsRoomOwner)
				{
					CurrentSave.CurrentValue?.Dispose();
					if (PhotonManager.Instance.GetRoomProperty<SaveInfoShort>("si", out var obj) && !obj.IsEmpty)
					{
						SaveInfo saveInfo = (SaveInfo)obj;
						m_CurrentSave.Value = new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>());
					}
				}
				SetPlayers();
			}
			else
			{
				PlayerVms.ForEach(delegate(NetLobbyPlayerVM vm)
				{
					vm.ClearPlayer();
				});
				ResetCurrentSave();
			}
		}).AddTo(this);
		IsHost.Subscribe(delegate
		{
			ResetCurrentSave();
		}).AddTo(this);
		NetGameCurrentState.Subscribe(delegate
		{
			UpdateRoom();
		}).AddTo(this);
		CurrentSave.Subscribe(delegate
		{
			DisposeSaveSloCollection();
		}).AddTo(this);
		SaveSlotCollectionVm.Subscribe(delegate
		{
			m_SaveSlotVMs.ForEach(delegate(SaveSlotVM vm)
			{
				vm.Dispose();
			});
			m_SaveSlotVMs.Clear();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdate();
		}).AddTo(this);
		if (!NetLobbyTutorialHasShown && IsAnyTutorialBlocks.CurrentValue)
		{
			ShowNetLobbyTutorial();
		}
		if (PlatformServices.Platform.HasSecondaryPlatform)
		{
			m_EpicGamesButtonActive.Value = PlatformServices.Platform.HasSecondaryPlatform;
			Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
			HandleSetEpicGamesUserName(secondaryPlatform.IsInitialized(), secondaryPlatform.User.NickName);
		}
		PhotonManager.IdleConnection.LobbyViewOpened = true;
		SetUserTypesDropdowns();
	}

	protected override void OnDispose()
	{
		PFLog.Net.Log("NET LOBBY STATE DISPOSE");
		HideScreenshot();
		DisposeSaveSloCollection();
		CurrentSave.CurrentValue?.Dispose();
		m_CurrentSave.Value = null;
		PlayerVms.ForEach(delegate(NetLobbyPlayerVM vm)
		{
			vm.Dispose();
		});
		PlayerVms.Clear();
		m_SaveSlotVMs.ForEach(delegate(SaveSlotVM vm)
		{
			vm.Dispose();
		});
		m_SaveSlotVMs.Clear();
		RegionDropdownVM.CurrentValue?.Dispose();
		m_RegionDropdownVM.Value = null;
		m_DifferentDlcWithSaveProblems?.Clear();
		DeactivateNetHandlers();
		PhotonManager.IdleConnection.LobbyViewOpened = false;
	}

	private void SetUserTypesDropdowns()
	{
	}

	private void SetUserTypeDropdown<TEnum>(string playerPrefName, Func<TEnum> getter, Action<TEnum> setter, out OwlcatDropdownVM dropdownVM, IEnumerable<TEnum> options, ReactiveProperty<TEnum> property, Func<TEnum, string> labelGetter, TEnum defaultValue)
	{
		int @int = PlayerPrefs.GetInt(playerPrefName, 0);
		setter(Enum.IsDefined(typeof(TEnum), @int) ? ((TEnum)(object)@int) : defaultValue);
		property.Value = getter();
		List<DropdownItemVM> vmCollection = options.Select((TEnum type) => new DropdownItemVM(labelGetter(type))).ToList();
		dropdownVM = new OwlcatDropdownVM(vmCollection).AddTo(this);
		property.Subscribe(setter).AddTo(this);
	}

	public void SetJoinableUserType(int type)
	{
		m_CurrentJoinableUserType.Value = (JoinableUserTypes)type;
	}

	public void SetInvitableUserType(int type)
	{
		m_CurrentInvitableUserType.Value = (InvitableUserTypes)type;
	}

	private void ActivateNetHandlers()
	{
		m_NetLobbyErrorHandler = new NetLobbyErrorHandler();
	}

	private void DeactivateNetHandlers()
	{
		m_NetLobbyErrorHandler.Dispose();
		m_NetLobbyErrorHandler = null;
	}

	private void DisposeSaveSloCollection()
	{
		SaveSlotCollectionVm.CurrentValue?.Dispose();
		m_SaveSlotCollectionVm.Value = null;
		m_ShowWaitingSaveAnim.Value = false;
		m_SaveListAreEmpty.Value = false;
	}

	private void SetRegions(RegionHandler regionHandler)
	{
		RegionDropdownVM.CurrentValue?.Dispose();
		m_RegionDropdownVM.Value = null;
		List<Region> enabledRegions = regionHandler.EnabledRegions;
		if (enabledRegions == null || enabledRegions.Count == 0)
		{
			return;
		}
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		int index = 0;
		for (int i = 0; i < enabledRegions.Count; i++)
		{
			Region region = enabledRegions[i];
			list.Add(new NetLobbyRegionDropdownVM($"{region.Code} - {region.Ping}ms", region.Code));
			if (PhotonManager.Instance.Region == region.Code)
			{
				index = i;
			}
		}
		m_RegionDropdownVM.Value = new OwlcatDropdownVM(list, index);
		RegionDropdownVM.CurrentValue.SelectedVM.Subscribe(delegate(DropdownItemVM itemVM)
		{
			NetLobbyRegionDropdownVM regionVM = itemVM as NetLobbyRegionDropdownVM;
			if (regionVM != null)
			{
				ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
				{
					PhotonManager.NetGame.ChangeRegion(regionVM.Region);
				});
			}
		}).AddTo(this);
	}

	private void OnUpdate()
	{
		m_ReadyToHostOrJoin.Value = PhotonManager.ReadyToHostOrJoin;
		if (m_NeedUpdateRegion && PhotonManager.Initialized && PhotonManager.Instance.RegionHandler != null && !PhotonManager.Instance.Region.IsNullOrEmpty())
		{
			SetRegions(PhotonManager.Instance.RegionHandler);
			m_NeedUpdateRegion = false;
		}
		SaveTransferUpdate();
	}

	private void UpdateRoom()
	{
		if (!(PhotonManager.Instance == null))
		{
			m_IsInRoom.Value = PhotonManager.Instance.InRoom;
			m_IsHost.Value = PhotonManager.Instance.IsRoomOwner;
			DisposeSaveSloCollection();
		}
	}

	public void OnClose()
	{
		if (SaveSlotCollectionVm.CurrentValue != null)
		{
			HideScreenshot();
			DisposeSaveSloCollection();
		}
		else if (PhotonManager.NetGame.CurrentState == NetGame.State.InLobby)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.LeaveLobbyMessageBox, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					Disconnect("OnClose");
					m_CloseAction?.Invoke();
				}
			});
		}
		else
		{
			m_CloseAction?.Invoke();
		}
	}

	public void CreateLobby()
	{
		PlayerPrefs.SetInt("net_lobby_selected_invitable_user", (int)PhotonManager.NetGame.CurrentInvitableUserType);
		PlayerPrefs.SetInt("net_lobby_selected_joinable_user", (int)PhotonManager.NetGame.CurrentJoinableUserType);
		PhotonManager.NetGame.CreateNewLobby();
	}

	public void JoinLobby()
	{
		NetRoomNameHelper.TryParse(m_Code, PhotonManager.Instance.RegionHandler.EnabledRegions, out var server, out var room);
		PhotonManager.Invite.AcceptInvite(server, room);
	}

	public void StopWaiting()
	{
		Disconnect("StopWaiting");
	}

	public void Disconnect(string reason)
	{
		PhotonManager.Instance.StopPlaying(reason);
	}

	public bool Launch()
	{
		if (CurrentSave.CurrentValue?.Reference != null)
		{
			bool num = CurrentSave.CurrentValue.Reference.CheckDlcAvailable();
			bool isDlcsInLobbyReady = PhotonManager.DLC.IsDLCsInLobbyReady;
			if (!num || !isDlcsInLobbyReady)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning((!isDlcsInLobbyReady) ? UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading : UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool flag = PhotonManager.NetGame.StartGame((SaveInfoKey)CurrentSave.CurrentValue.Reference);
			if (flag)
			{
				UISounds.Instance.Sounds.Buttons.FinishChargenButtonClick.Play();
			}
			m_CanConfirmLaunch.Value = flag;
			return flag;
		}
		if ((PhotonManager.Sync.HasDesync || IsSaveAllowed.CurrentValue || CurrentSave.CurrentValue?.Reference != null) && !IsMainMenu)
		{
			if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool num2 = Game.Instance.Player.CheckDlcAvailable();
			bool isDlcsInLobbyReady = PhotonManager.DLC.IsDLCsInLobbyReady;
			bool flag2 = m_DifferentDlcWithSaveProblems.Values.Any((List<IBlueprintDlc> v) => Game.Instance.Player.DlcRewardsToSave.Any((BlueprintDlcReward dr) => dr.Dlcs.Any(v.Contains)));
			if (!num2 || !isDlcsInLobbyReady || flag2)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning((!isDlcsInLobbyReady) ? UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading : UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool flag3 = PhotonManager.NetGame.StartGameWithoutSave();
			m_CanConfirmLaunch.Value = flag3;
			return flag3;
		}
		ChooseSave();
		m_CanConfirmLaunch.Value = false;
		return false;
	}

	public void SetLobbyCode(string code)
	{
		m_HasCodeForLobby.Value = PhotonManager.Initialized && PhotonManager.Instance.RegionHandler != null && NetRoomNameHelper.Check(code, PhotonManager.Instance.RegionHandler.EnabledRegions);
		m_Code = code;
	}

	public void CopyLobbyId()
	{
		NetRoomNameHelper.TryFormatString(PhotonManager.Instance.Region, PhotonManager.Instance.RoomName, out var output);
		GUIUtility.systemCopyBuffer = output;
	}

	public string GetCopiedLobbyId()
	{
		return GUIUtility.systemCopyBuffer;
	}

	public void ChooseSave()
	{
		m_SaveSlotCollectionVm.Value = new SaveSlotCollectionVM(SaveLoadMode, CurrentSave, OnClose);
		Game.Instance.SaveManager.UpdateSaveListAsync();
		m_ShowWaitingSaveAnim.Value = true;
	}

	public void ResetCurrentSave()
	{
		CurrentSave.CurrentValue?.Dispose();
		m_CurrentSave.Value = null;
		if (PhotonManager.Instance != null && PhotonManager.Instance.IsRoomOwner)
		{
			PhotonManager.Save.SelectSave(null);
		}
	}

	public void HandleTransferProgressChanged(bool value)
	{
		m_IsSaveTransfer.Value = value;
		SaveTransferUpdate();
	}

	private void SaveTransferUpdate()
	{
		if (IsSaveTransfer.CurrentValue && PhotonManager.Save.GetSentProgress(out var progress, out var target))
		{
			m_SaveTransferProgress.Value = progress;
			m_SaveTransferTarget.Value = target;
		}
	}

	private void SetPlayers()
	{
		ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
		for (int i = 0; i < PlayerVms.Count; i++)
		{
			if (i < allPlayers.Count)
			{
				PlayerVms[i].SetPlayer(allPlayers[i].Player, allPlayers[i].UserId, allPlayers[i].IsActive);
			}
			else
			{
				PlayerVms[i].ClearPlayer();
			}
		}
		CheckEnoughPlayers();
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		m_NetGameCurrentState.Value = state;
		m_NetGameCurrentState.ForceNotify();
		m_IsPlayingState.Value = state == NetGame.State.Playing;
		PFLog.Net.Log($"NET LOBBY STATE CHANGE: {state}");
		if (state == NetGame.State.InLobby)
		{
			m_CanConfirmLaunch.Value = false;
		}
	}

	public void HandleNLoadingScreenClosed()
	{
		m_CanConfirmLaunch.Value = false;
	}

	public void OnFireTrigger(NetGame.Trigger trigger)
	{
	}

	public void OnStateChanged(NetGame.State oldState, NetGame.State newState)
	{
		PFLog.Net.Log($"NET LOBBY STATE CHANGE: {oldState} -> {newState}");
		if (newState == NetGame.State.InLobby)
		{
			m_CanConfirmLaunch.Value = false;
		}
	}

	public void OnProcessTrigger(NetGame.Trigger trigger, NetGame.State currentState, NetGame.State nextState)
	{
	}

	public void OnFireException(Exception exception)
	{
	}

	public void OnUnhandledTransition(NetGame.Trigger trigger, NetGame.State currentState)
	{
	}

	public void OnIgnoreTrigger(NetGame.Trigger trigger, NetGame.State currentState)
	{
	}

	public void OnSaveListUpdated()
	{
		if (SaveSlotCollectionVm.CurrentValue == null)
		{
			return;
		}
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			m_ShowWaitingSaveAnim.Value = false;
			ProblemsToShowInSaveList.Clear();
			List<SaveInfo> referenceCollection = new List<SaveInfo>(Game.Instance.SaveManager);
			referenceCollection.RemoveAll(SaveManager.IsCoopSave);
			referenceCollection.RemoveAll((SaveInfo si) => si != null && si.Type == SaveInfo.SaveType.IronMan);
			if (m_DifferentDlcWithSaveProblems.Any())
			{
				List<SaveInfo> source = referenceCollection.Where((SaveInfo s) => s.DlcRewards != null && s.DlcRewards.Any()).ToList();
				IEnumerable<SaveInfo> savesToHide = source.Where((SaveInfo s) => s.GetRequiredDLCMap().Contains((List<IBlueprintDlc> requiredDLCList) => requiredDLCList.Contains(delegate(IBlueprintDlc requiredDLC)
				{
					IEnumerable<KeyValuePair<string, List<IBlueprintDlc>>> enumerable = m_DifferentDlcWithSaveProblems.Where((KeyValuePair<string, List<IBlueprintDlc>> i) => i.Value.Contains(requiredDLC) || requiredDLC.Rewards.Any(delegate(IBlueprintDlcReward r)
					{
						BlueprintDlcReward reward = r as BlueprintDlcReward;
						return reward != null && i.Value.Any((IBlueprintDlc item) => reward.Dlcs.Contains(item));
					}));
					if (!enumerable.Any())
					{
						return false;
					}
					foreach (KeyValuePair<string, List<IBlueprintDlc>> pd in enumerable)
					{
						if (!ProblemsToShowInSaveList.ContainsKey(pd.Key))
						{
							ProblemsToShowInSaveList[pd.Key] = pd.Value;
						}
						else
						{
							foreach (IBlueprintDlc item in pd.Value.Where((IBlueprintDlc value) => !ProblemsToShowInSaveList[pd.Key].Contains(value)))
							{
								ProblemsToShowInSaveList[pd.Key].Add(item);
							}
						}
					}
					return true;
				})));
				referenceCollection.RemoveAll((SaveInfo r) => savesToHide.Contains(r));
			}
			referenceCollection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
			bool allowSwitchOff = Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad;
			foreach (SaveInfo saveInfo in referenceCollection)
			{
				if (!m_SaveSlotVMs.Any((SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
				{
					SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, SaveLoadMode, new SaveLoadActions
					{
						Select = delegate(SaveInfo info)
						{
							if (!allowSwitchOff)
							{
								SetSelectSaveAction(info);
							}
						},
						SaveOrLoad = delegate(SaveInfo info)
						{
							if (allowSwitchOff)
							{
								SetSelectSaveAction(info);
							}
						},
						ShowScreenshot = RequestShowScreenshot
					}, allowSwitchOff);
					saveSlotVM.AddTo(this);
					SaveSlotCollectionVm.CurrentValue.HandleNewSave(saveSlotVM);
					m_SaveSlotVMs.Add(saveSlotVM);
				}
			}
			List<SaveSlotVM> list = new List<SaveSlotVM>();
			foreach (SaveSlotVM item2 in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !referenceCollection.Any(saveSlotVm.ReferenceSaveEquals)))
			{
				item2.Dispose();
				list.Add(item2);
			}
			foreach (SaveSlotVM item3 in list)
			{
				SaveSlotCollectionVm.CurrentValue.HandleDeleteSave(item3);
				m_SaveSlotVMs.Remove(item3);
			}
			m_SaveListAreEmpty.Value = m_SaveSlotVMs.Count == 0;
		}));
	}

	private void RequestShowScreenshot(SaveSlotVM saveSlotVM)
	{
		saveSlotVM?.UpdateHighResScreenshot();
		m_SaveFullScreenshot.Value = saveSlotVM;
	}

	private void HideScreenshot()
	{
		m_SaveFullScreenshot.Value = null;
	}

	private void SetSelectSaveAction(SaveInfo saveInfo)
	{
		bool num = saveInfo != null && saveInfo.Type == SaveInfo.SaveType.IronMan;
		if (IsMainMenu)
		{
			_ = 1;
		}
		else
			_ = !SettingsRoot.Difficulty.OnlyOneSave;
		if (num)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			SetSave(saveInfo);
		}
	}

	private void SetSave(SaveInfo saveInfo)
	{
		SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>());
		if (saveSlotVM.Reference != null && !saveSlotVM.Reference.CheckDlcAvailable())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			m_CurrentSave.Value = saveSlotVM;
			PhotonManager.Save.SelectSave(saveInfo);
		}
	}

	public void OnAreaBeginUnloading()
	{
		m_CloseAction?.Invoke();
	}

	public void OnAreaDidLoad()
	{
		m_CloseAction?.Invoke();
	}

	public void RequestRole()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		SetPlayers();
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		SetPlayers();
	}

	public void HandlePlayerChanged()
	{
		SetPlayers();
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
		UpdateRoom();
	}

	public void HandleSaveSelect(SaveInfo saveInfo)
	{
		if (!IsHost.CurrentValue)
		{
			CurrentSave.CurrentValue?.Dispose();
			m_CurrentSave.Value = ((saveInfo != null) ? new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>()) : null);
		}
	}

	public void ShowNetLobbyTutorial()
	{
		m_NetLobbyTutorialPartVM.Value = new NetLobbyTutorialPartVM(delegate
		{
			HideNetLobbyTutorial();
		});
		m_NetLobbyTutorialOnScreen.Value = true;
		void HideNetLobbyTutorial()
		{
			SetFirstLaunchPrefs();
			NetLobbyTutorialPartVM.CurrentValue?.Dispose();
			m_NetLobbyTutorialPartVM.Value = null;
			m_NetLobbyTutorialOnScreen.Value = false;
		}
	}

	[Cheat(Name = "clear_net_lobby_tutorial")]
	public static void ClearFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_net_lobby_tutorial", 0);
		PlayerPrefs.Save();
	}

	[Cheat(Name = "set_net_lobby_tutorial")]
	public static void SetFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_net_lobby_tutorial", 1);
		PlayerPrefs.Save();
	}

	private void CheckEnoughPlayers()
	{
		m_IsEnoughPlayersForGame.Value = PhotonManager.Instance.IsEnoughPlayersForGame;
	}

	public async void OpenEpicGamesLayer()
	{
		Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
		if (!secondaryPlatform.IsInitialized())
		{
			await PlatformServices.Platform.InitSecondary();
		}
		secondaryPlatform.Invite.ShowInviteWindow();
	}

	private void CheckProblemsWithMods()
	{
		Dictionary<string, ModData[]> dictionary = new Dictionary<string, ModData[]>();
		foreach (PlayerInfo allPlayer in PhotonManager.Instance.AllPlayers)
		{
			PhotonManager.Mods.TryGetModsData(allPlayer.UserId, out var mods);
			dictionary.TryAdd(allPlayer.UserId, mods);
		}
		List<ModData[]> list = (from pair in dictionary
			select new
			{
				UserId = pair.Key,
				Mods = pair.Value
			} into user
			group user by user.Mods).SelectMany(group => group.Select(user => user.Mods)).ToList();
		if (!list.Any())
		{
			m_PlayersDifferentMods.Value = string.Empty;
			return;
		}
		List<string> list2 = new List<string>();
		int num = 0;
		foreach (ModData[] item2 in list)
		{
			if (item2 == null || !item2.Any())
			{
				continue;
			}
			ModData[] array = item2;
			foreach (ModData mod in array)
			{
				if (!list2.Any((string s) => s.Contains(mod.ToString())))
				{
					string item = $"{num + 1}. {mod}";
					list2.Add(item);
					num++;
				}
			}
		}
		m_PlayersDifferentMods.Value = ((!list2.Any()) ? string.Empty : (UIStrings.Instance.NetLobbyTexts.CanBeAProblemsWithMods.Text + ":" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, list2)));
	}

	public void ShowDlcList()
	{
		PlayerInfo playerInfo = PhotonManager.Instance.AllPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (!string.IsNullOrWhiteSpace(playerInfo.UserId))
		{
			ReactiveProperty<NetLobbyDlcListVM> dlcListVM = m_DlcListVM;
			if (dlcListVM.Value == null)
			{
				NetLobbyDlcListVM netLobbyDlcListVM2 = (dlcListVM.Value = new NetLobbyDlcListVM(CloseAction, GetPlayerDlcs(playerInfo.UserId)));
			}
		}
		void CloseAction()
		{
			DlcListVM.CurrentValue?.Dispose();
			m_DlcListVM.Value = null;
		}
	}

	private List<IBlueprintDlc> GetPlayerDlcs(string userID)
	{
		if (PhotonManager.DLC.TryGetPlayerDLC(userID, out var playerDLCs))
		{
			return playerDLCs;
		}
		PFLog.UI.Log("[NetLobbyPlayerVM.RefreshDLCsList] DLCs for user='" + userID + "' not found!");
		return new List<IBlueprintDlc>();
	}

	private void CompareHostAndPlayerDlcs()
	{
		m_DifferentDlcWithSaveProblems.Clear();
		ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
		PlayerInfo playerInfo = allPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (string.IsNullOrWhiteSpace(playerInfo.UserId))
		{
			m_CheckProblemsWithDlcs.Execute(m_DifferentDlcWithSaveProblems);
			return;
		}
		List<BlueprintDlc> first = GetPlayerDlcs(playerInfo.UserId).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType = dlc.DlcType;
			return dlcType != DlcTypeEnum.CosmeticDlc && dlcType != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		foreach (PlayerInfo item in allPlayers)
		{
			if (playerInfo.UserId == item.UserId)
			{
				continue;
			}
			List<IBlueprintDlc> playerDlcs = GetPlayerDlcs(item.UserId);
			List<IBlueprintDlc> list = first.Except(playerDlcs).ToList();
			if (list.Any())
			{
				string nickName;
				string key = ((PhotonManager.Player.GetNickName(item.Player, out nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : item.UserId);
				if (!m_DifferentDlcWithSaveProblems.ContainsKey(key))
				{
					m_DifferentDlcWithSaveProblems.Add(key, list);
				}
				else
				{
					m_DifferentDlcWithSaveProblems[key].AddRange(list);
				}
			}
		}
		m_CheckProblemsWithDlcs.Execute(m_DifferentDlcWithSaveProblems);
	}

	public void HandleSetEpicGamesButtonActive(bool state)
	{
		m_EpicGamesButtonActive.Value = state;
	}

	public void HandleSetEpicGamesUserName(bool isAuthorized, string name)
	{
		m_EpicGamesAuthorized.Value = isAuthorized;
		m_EpicGamesUserName.Value = name;
	}

	public void HandleCheckUsersMods()
	{
		CheckProblemsWithMods();
		CompareHostAndPlayerDlcs();
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
	}

	public void HandleNetLobbyClose()
	{
		OnClose();
	}
}
