using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.UI.MVVM.Vendor;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Pointer;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CommonVM : ViewModel, IGameModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ICounterWindowUIHandler, IContextMenuHandler, IFullScreenUIHandler, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, IDlcManagerUIHandler, ITradeStateChanged, ISubscriber<IMechanicEntity>, IBeginSelectingVendorHandler, ICreditsWindowUIHandler, INetLobbyRequest, INetRolesRequest, INetInviteHandler, INetLobbyPlayersHandler
{
	private readonly ReactiveProperty<NetLobbyVM> m_NetLobbyVM = new ReactiveProperty<NetLobbyVM>(null);

	private readonly ReactiveProperty<NetRolesVM> m_NetRolesVM = new ReactiveProperty<NetRolesVM>(null);

	private readonly ReactiveProperty<DlcManagerVM> m_DlcManagerVM = new ReactiveProperty<DlcManagerVM>(null);

	private readonly ReactiveProperty<TransitionVM> m_TransitionVM = new ReactiveProperty<TransitionVM>(null);

	private readonly ReactiveProperty<CreditsVM> m_CreditsVM = new ReactiveProperty<CreditsVM>();

	private readonly ReactiveProperty<VendorBaseScreenVM> m_VendorVM = new ReactiveProperty<VendorBaseScreenVM>();

	private readonly ReactiveProperty<VendorSelectingWindowVM> m_VendorSelectingWindowVM = new ReactiveProperty<VendorSelectingWindowVM>();

	private readonly ReactiveProperty<ContextMenuVM> m_ContextMenuVM = new ReactiveProperty<ContextMenuVM>();

	private readonly ReactiveProperty<CounterWindowVM> m_CounterWindowVM = new ReactiveProperty<CounterWindowVM>();

	public readonly FadeVM FadeVM;

	public readonly UIVisibilityVM UIVisibilityVM;

	private readonly Queue<MessageBoxVM> m_MessageQueue = new Queue<MessageBoxVM>();

	public readonly CurrentUnitCombatVM CurrentUnitCombatVM;

	private readonly ReactiveProperty<bool> m_IsCombatInputModeActive = new ReactiveProperty<bool>();

	private Action<MechanicEntity> m_BeginTradingAction;

	public ReadOnlyReactiveProperty<TransitionVM> TransitionVM => m_TransitionVM;

	public ReadOnlyReactiveProperty<NetLobbyVM> NetLobbyVM => m_NetLobbyVM;

	public ReadOnlyReactiveProperty<NetRolesVM> NetRolesVM => m_NetRolesVM;

	public ReadOnlyReactiveProperty<DlcManagerVM> DlcManagerVM => m_DlcManagerVM;

	public ReadOnlyReactiveProperty<CreditsVM> CreditsVM => m_CreditsVM;

	public ReadOnlyReactiveProperty<VendorBaseScreenVM> VendorVM => m_VendorVM;

	public ReadOnlyReactiveProperty<VendorSelectingWindowVM> VendorSelectingWindowVM => m_VendorSelectingWindowVM;

	public ReadOnlyReactiveProperty<CounterWindowVM> CounterWindowVM => m_CounterWindowVM;

	public ReadOnlyReactiveProperty<ContextMenuVM> ContextMenuVM => m_ContextMenuVM;

	public ReadOnlyReactiveProperty<bool> IsCombatInputModeActive => m_IsCombatInputModeActive;

	public bool IsInQuestNotification => QuestNotificatorVM.Instance?.IsShowUp.CurrentValue ?? false;

	public CommonVM()
	{
		UIVisibilityVM = new UIVisibilityVM().AddTo(this);
		FadeVM = new FadeVM().AddTo(this);
		CurrentUnitCombatVM = new CurrentUnitCombatVM().AddTo(this);
		m_IsCombatInputModeActive.Value = TurnController.IsInTurnBasedCombat();
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SwitchUIVisibility.name, UIVisibilityState.SwitchVisibility).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeCounterWindow();
		DisposeContextMenu();
		DisposeNetLobby();
		DisposeNetRoles();
	}

	private void ForceDisposeAllFullscreen()
	{
		TooltipsDataCache.Instance?.Clear();
		DisposeNetLobby();
		DisposeContextMenu();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_IsCombatInputModeActive.Value = isTurnBased;
		if (!isTurnBased && (BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			WidgetFactoryStash.ResetStash();
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		m_IsCombatInputModeActive.Value = true;
	}

	public void OpenJournal()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenJournal();
		});
	}

	public void QuestNotificationForceClose()
	{
		QuestNotificatorVM.Instance.ForceClose();
	}

	public void HandleShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void CloseTutorialOnLoad()
	{
		if (Game.Instance.TutorialSystem.HasShownData)
		{
			EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
			{
				h.HideTutorial(Game.Instance.TutorialSystem.ShowingData);
			});
		}
	}

	public void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command)
	{
		DisposeCounterWindow();
		m_CounterWindowVM.Value = new CounterWindowVM(type, item, command, DisposeCounterWindow).AddTo(this);
	}

	public void HandleCloseCounterWindow()
	{
		DisposeCounterWindow();
	}

	private void DisposeCounterWindow()
	{
		CounterWindowVM.CurrentValue?.Dispose();
		m_CounterWindowVM.Value = null;
	}

	public void HandleContextMenuRequest(IContextMenuCollection collection)
	{
		DisposeContextMenu();
		if (collection != null)
		{
			m_ContextMenuVM.Value = new ContextMenuVM(collection).AddTo(this);
		}
	}

	private void DisposeContextMenu()
	{
		ContextMenuVM.CurrentValue?.Dispose();
		m_ContextMenuVM.Value = null;
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		if (NetLobbyVM.CurrentValue == null)
		{
			m_NetLobbyVM.Value = new NetLobbyVM(delegate
			{
				DisposeNetLobby(isMainMenu);
			});
			if (isMainMenu)
			{
				SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.CoopLobby);
			}
		}
	}

	public void HandleNetLobbyClose()
	{
	}

	private void DisposeNetLobby(bool isMainMenu = false)
	{
		NetLobbyVM.CurrentValue?.Dispose();
		m_NetLobbyVM.Value = null;
		if (isMainMenu)
		{
			SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.MainMenu);
		}
	}

	public void HandleOpenDlcManager(bool inGame = false)
	{
		if (DlcManagerVM.CurrentValue == null)
		{
			m_DlcManagerVM.Value = new DlcManagerVM(DisposeDlcManager, inGame);
		}
	}

	public void HandleCloseDlcManager()
	{
	}

	private void DisposeDlcManager()
	{
		DlcManagerVM.CurrentValue?.Dispose();
		m_DlcManagerVM.Value = null;
		EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
		{
			h.HandleCloseDlcManager();
		});
	}

	public void HandleNetRolesRequest()
	{
		m_NetRolesVM.Value = new NetRolesVM(DisposeNetRoles);
	}

	private void DisposeNetRoles()
	{
		NetRolesVM.CurrentValue?.Dispose();
		m_NetRolesVM.Value = null;
	}

	public void HandleInvite(Action<bool> callback)
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.InviteLobbyMessageBox, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			callback?.Invoke(button == DialogMessageBoxButton.Yes);
		});
	}

	public void HandleInviteAccepted(bool accepted)
	{
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		CoroutineRunner.Start(SendWarningCoroutine(player));
		static float CurrentTimeSeconds()
		{
			return Time.realtimeSinceStartup;
		}
		static IEnumerator SendWarningCoroutine(Photon.Realtime.Player player)
		{
			float timeoutSeconds = CurrentTimeSeconds() + 2f;
			while (string.IsNullOrEmpty(player.NickName))
			{
				yield return null;
				if (timeoutSeconds < CurrentTimeSeconds())
				{
					yield break;
				}
			}
			UtilityMessageBox.SendWarning(string.Format((PhotonManager.NetGame.CurrentState != NetGame.State.Playing) ? UIStrings.Instance.NetLobbyTexts.NewPlayerJoinToLobby : UIStrings.Instance.NetLobbyTexts.NewPlayerJoinToActiveLobby, player.NickName));
		}
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		if (PhotonManager.NetGame.CurrentState == NetGame.State.Playing)
		{
			UtilityMessageBox.SendWarning(string.Format(UIStrings.Instance.NetLobbyTexts.PlayerLeftRoomWarning, player.NickName));
		}
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.LastPlayerLeftLobbyMessageBox, DialogMessageBoxType.Message, delegate
		{
		});
	}

	public void HandleRoomOwnerChanged()
	{
		if (PhotonManager.Instance.IsRoomOwner)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.YouAreTheHostNow, DialogMessageBoxType.Message, delegate
			{
			});
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		_ = Game.Instance;
		if (fullScreenUIType != FullScreenUIType.LocalMap && NetLobbyVM.CurrentValue == null && NetRolesVM.CurrentValue == null && RootVM.Instance.SaveLoadVM.CurrentValue == null && RootVM.Instance.SettingsVM.CurrentValue == null && RootVM.Instance.BugReportVM.CurrentValue == null)
		{
			_ = DlcManagerVM.CurrentValue;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap || gameMode == GameModeType.StarSystem || gameMode == GameModeType.Dialog)
		{
			ForceDisposeAllFullscreen();
		}
		if (!(gameMode != GameModeType.GameOver))
		{
			RootVM.Instance.HUDContext?.InspectVM.CurrentValue?.HandleShowInspect(state: false);
			TooltipHelper.HideTooltip();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		ForceDisposeAllFullscreen();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		ForceDisposeAllFullscreen();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			ForceDisposeAllFullscreen();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleOpenCredits()
	{
		m_CreditsVM.Value = new CreditsVM(DisposeCredits).AddTo(this);
		void DisposeCredits()
		{
			CreditsVM.CurrentValue?.Dispose();
			m_CreditsVM.Value = null;
		}
	}

	void ITradeStateChanged.HandleBeginTrading()
	{
		ForceDisposeAllFullscreen();
		m_VendorVM.Value = new VendorBaseScreenVM().AddTo(this);
	}

	void ITradeStateChanged.HandleEndTrading()
	{
		DisposeVendor();
	}

	void ITradeStateChanged.HandleVendorAboutToTrading()
	{
	}

	private void DisposeVendor()
	{
		VendorVM.CurrentValue?.Dispose();
		m_VendorVM.Value = null;
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		ForceDisposeAllFullscreen();
		bool flag = true;
		if ((bool)ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current)
		{
			EntityRef<BaseUnitEntity> entityRef = ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current.EntityRef;
			flag = entityRef.IsNull || (!entityRef.IsNull && ((BaseUnitEntity)entityRef).IsDirectlyControllable());
		}
		if (flag)
		{
			m_TransitionVM.Value = new TransitionVM(multiEntrance, DisposeTransition).AddTo(this);
		}
	}

	private void DisposeTransition()
	{
		TransitionVM.CurrentValue?.Dispose();
		m_TransitionVM.Value = null;
	}

	public void HandleBeginSelectingVendor(List<MechanicEntity> vendors)
	{
		m_VendorSelectingWindowVM.Value = new VendorSelectingWindowVM(vendors).AddTo(this);
	}

	public void HandleExitSelectingVendor()
	{
		VendorSelectingWindowVM.CurrentValue?.Dispose();
		m_VendorSelectingWindowVM.Value = null;
	}
}
