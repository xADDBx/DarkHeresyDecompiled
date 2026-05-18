using System;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.CombatNotifications;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using Kingmaker.Code.UI.MVVM.SkipCutscene;
using Kingmaker.Code.UI.MVVM.UnitInfo;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Code.View.UI.UIUtils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class HUDContext : ViewModel, IAreaActivationHandler, ISubscriber, IAreaHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IGameModeHandler, INetRoleSetHandler, INetEvents, IMoraleVictoryConfirmationRequest, IInteractionHighlightUIHandler, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandEndHandler, ITurnStartHandler
{
	private IDisposable m_ReleasePartyUnitCommand;

	private IDisposable m_TrackerSubscription;

	private IDisposable m_PendingCombatEndShow;

	private bool m_ComponentsVMsCreated;

	private Vector3 m_CachedWorldPosition;

	private bool m_IsMoraleVictory;

	private readonly ReactiveProperty<bool> m_CanDeploy = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_PlayerHaveRoles = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NetFirstLoadState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ForceHotKeyPressed = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsTurnBasedActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowEndTurn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanEndTurn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DeploymentPhase = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_PartyUnitIsRunningCommand = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentUnit = new ReactiveProperty<CombatMechanicEntityVM>();

	private readonly ReactiveProperty<bool> m_CombatIsStarting = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_CombatIsEnding = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<InitiativeTrackerVM> m_InitiativeTrackerVM;

	private readonly ReactiveProperty<CombatStartWindowVM> m_CombatStartWindowVM;

	private readonly ReactiveProperty<CombatStartNotificationVM> m_CombatStartNotificationVM;

	private readonly ReactiveProperty<CombatHUDNotificationsVM> m_CombatHUDNotificationVM;

	private readonly ReactiveProperty<ActionBarVM> m_ActionBarVM;

	private readonly ReactiveProperty<InspectVM> m_InspectVM;

	private readonly ReactiveProperty<CombatLogVM> m_CombatLogVM;

	private readonly ReactiveProperty<IngameMenuVM> m_IngameMenuVM;

	private readonly ReactiveProperty<IngameMenuSettingsButtonVM> m_IngameMenuSettingsButtonVM;

	private readonly ReactiveProperty<PartyVM> m_PartyVM;

	private readonly ReactiveProperty<PreciseAttackVM> m_PreciseAttackVM;

	private readonly ReactiveProperty<UnitInfoVM> m_UnitInfoVM;

	private readonly ReactiveProperty<SignalsDeviceVM> m_SignalDeviceVM;

	private readonly ReactiveProperty<SkipCutsceneVM> m_SkipCutsceneVM;

	private readonly ReactiveProperty<CombatEndWindowVM> m_CombatEndWindowVM;

	private BaseUnitEntity SingleSelectedUnit => Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Value;

	public ReadOnlyReactiveProperty<bool> IsTurnBasedActive => m_IsTurnBasedActive;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public ReadOnlyReactiveProperty<bool> ShowEndTurn => m_ShowEndTurn;

	public ReadOnlyReactiveProperty<bool> CanEndTurn => m_CanEndTurn;

	public ReadOnlyReactiveProperty<bool> DeploymentPhase => m_DeploymentPhase;

	public ReadOnlyReactiveProperty<bool> ForceHotKeyPressed => m_ForceHotKeyPressed;

	public ReadOnlyReactiveProperty<bool> PartyUnitIsRunningCommand => m_PartyUnitIsRunningCommand;

	public ReadOnlyReactiveProperty<InitiativeTrackerVM> InitiativeTrackerVM => m_InitiativeTrackerVM;

	public ReadOnlyReactiveProperty<ActionBarVM> ActionBarVM => m_ActionBarVM;

	public ReadOnlyReactiveProperty<CombatEndWindowVM> CombatEndWindowVM => m_CombatEndWindowVM;

	public Vector3 PointerWorldPosition
	{
		get
		{
			if (!ForceHotKeyPressed.CurrentValue)
			{
				return Game.Instance.Controllers.ClickEventsController.WorldPosition;
			}
			return m_CachedWorldPosition;
		}
	}

	public HUDContext(ReactiveProperty<CombatStartWindowVM> combatStartWindowVM, ReactiveProperty<CombatStartNotificationVM> combatStartNotificationVM, ReactiveProperty<CombatHUDNotificationsVM> combatHUDNotificationsVM, ReactiveProperty<InitiativeTrackerVM> initiativeTrackerVM, ReactiveProperty<ActionBarVM> actionBarVM, ReactiveProperty<InspectVM> inspectVM, ReactiveProperty<CombatLogVM> combatLogVM, ReactiveProperty<IngameMenuVM> ingameMenuVM, ReactiveProperty<IngameMenuSettingsButtonVM> ingameMenuSettingsButtonVM, ReactiveProperty<PartyVM> partyVM, ReactiveProperty<PreciseAttackVM> preciseAttackVM, ReactiveProperty<UnitInfoVM> unitInfoVM, ReactiveProperty<SignalsDeviceVM> signalDeviceVM, ReactiveProperty<SkipCutsceneVM> skipCutsceneVM, ReactiveProperty<CombatEndWindowVM> combatEndWindowVM)
	{
		m_CombatStartWindowVM = combatStartWindowVM;
		m_CombatStartNotificationVM = combatStartNotificationVM;
		m_CombatHUDNotificationVM = combatHUDNotificationsVM;
		m_InitiativeTrackerVM = initiativeTrackerVM;
		m_ActionBarVM = actionBarVM;
		m_InspectVM = inspectVM;
		m_CombatLogVM = combatLogVM;
		m_IngameMenuVM = ingameMenuVM;
		m_IngameMenuSettingsButtonVM = ingameMenuSettingsButtonVM;
		m_PartyVM = partyVM;
		m_PreciseAttackVM = preciseAttackVM;
		m_UnitInfoVM = unitInfoVM;
		m_SignalDeviceVM = signalDeviceVM;
		m_SkipCutsceneVM = skipCutsceneVM;
		m_CombatEndWindowVM = combatEndWindowVM;
		EventBus.Subscribe(this).AddTo(this);
		UpdateIsTurnBasedActive();
		TurnBasedModeChanged(IsTurnBasedActive.CurrentValue);
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			BeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateIsTurnBasedActive();
		}).AddTo(this);
		IsTurnBasedActive.Subscribe(TurnBasedModeChanged).AddTo(this);
		Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}).AddTo(this);
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
		Game.Instance.Controllers.PreciseAttackController.Show.Subscribe(HandlePreciseAttackUIShow).AddTo(this);
		TryCreateVMs();
	}

	public void EndTurn()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController != null && turnController.TurnBasedModeActive && (!turnController.IsSpaceCombat || UtilityNet.IsControlMainCharacter()) && !turnController.IsPreparationTurn && turnController.IsPlayerTurn && turnController.CurrentUnit.IsMyNetRole() && turnController.CurrentUnit.GetCommandsOptional()?.Current == null && !turnController.IsPreciseAttack)
		{
			turnController.TryEndPlayerTurnManually();
		}
	}

	protected override void OnDispose()
	{
		m_PendingCombatEndShow?.Dispose();
		EndPreparationTurn();
		m_CurrentUnit.ClearDisposableValue();
		m_CombatHUDNotificationVM.ClearDisposableValue();
		m_CombatStartNotificationVM.ClearDisposableValue();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		TryCreateVMs();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		DisposeAll();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
	}

	void IPreparationTurnBeginHandler.HandleBeginPreparationTurn(bool canDeploy)
	{
		BeginPreparationTurn(canDeploy);
	}

	void IPreparationTurnEndHandler.HandleEndPreparationTurn()
	{
		EndPreparationTurn();
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			ShowCombatStartNotification();
			return;
		}
		EndPreparationTurn();
		if (!m_IsMoraleVictory)
		{
			m_PendingCombatEndShow?.Dispose();
			m_PendingCombatEndShow = DelayedInvoker.InvokeInFrames(delegate
			{
				if (!(Game.Instance.CurrentModeType != GameModeType.Default))
				{
					ShowCombatEnd(CombatEndReason.RegularVictory);
				}
			}, 1);
		}
		m_IsMoraleVictory = false;
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			BeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
		}
	}

	void IGameModeHandler.OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_SkipCutsceneVM.Value = new SkipCutsceneVM().AddTo(this);
			EndPreparationTurn();
		}
		OnUnitChanged();
	}

	void IGameModeHandler.OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_SkipCutsceneVM.CurrentValue?.Dispose();
			m_SkipCutsceneVM.Value = null;
			if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
			{
				BeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
			}
		}
		OnUnitChanged();
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		m_PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	void INetEvents.HandleTransferProgressChanged(bool value)
	{
	}

	void INetEvents.HandleNetGameStateChanged(NetGame.State state)
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	void INetEvents.HandleNLoadingScreenClosed()
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	void IMoraleVictoryConfirmationRequest.HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		CameraFollowTaskParams toTargetOnMoraleVictory = ConfigRoot.Instance.CameraRoot.CameraFollowSettings.Get().ToTargetOnMoraleVictory;
		float seconds = toTargetOnMoraleVictory.CameraObserveTime + toTargetOnMoraleVictory.BlendSettings.BlendTime;
		m_PendingCombatEndShow?.Dispose();
		m_PendingCombatEndShow = DelayedInvoker.InvokeInTime(delegate
		{
			ShowCombatEnd(CombatEndReason.MoraleVictory, delegate(bool confirmed)
			{
				callback(confirmed);
			});
		}, seconds);
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		m_ForceHotKeyPressed.Value = isOn;
		m_CachedWorldPosition = Game.Instance.Controllers.ClickEventsController.WorldPosition;
	}

	void IUnitCommandStartHandler.HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor.IsInPlayerParty && (command.Executor == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity || command.Target == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity))
		{
			m_ReleasePartyUnitCommand?.Dispose();
			m_PartyUnitIsRunningCommand.Value = true;
		}
	}

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor.IsInPlayerParty && (command.Executor == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity || command.Target == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity))
		{
			m_ReleasePartyUnitCommand = ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(2), delegate
			{
				m_PartyUnitIsRunningCommand.Value = false;
			}).AddTo(this);
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity == null || !mechanicEntity.IsInPlayerParty)
		{
			m_ReleasePartyUnitCommand?.Dispose();
			m_PartyUnitIsRunningCommand.Value = false;
		}
	}

	private void DisposeAll()
	{
		m_ActionBarVM.ClearDisposableValue();
		m_InspectVM.ClearDisposableValue();
		m_CombatLogVM.ClearDisposableValue();
		m_IngameMenuVM.ClearDisposableValue();
		m_IngameMenuSettingsButtonVM.ClearDisposableValue();
		m_PartyVM.ClearDisposableValue();
		m_UnitInfoVM.ClearDisposableValue();
		m_SignalDeviceVM.ClearDisposableValue();
		m_CombatStartNotificationVM.ClearDisposableValue();
		m_ComponentsVMsCreated = false;
		m_CombatIsStarting.Value = false;
		m_CombatIsEnding.Value = false;
	}

	private void TryCreateVMs()
	{
		if (Game.Instance.LoadedArea == null)
		{
			DisposeAll();
		}
		else if (!m_ComponentsVMsCreated)
		{
			m_ActionBarVM.Value = new ActionBarVM(m_CurrentUnit, m_CombatIsStarting).AddTo(this);
			m_InspectVM.Value = new InGameInspectVM().AddTo(this);
			m_CombatLogVM.Value = new CombatLogVM().AddTo(this);
			m_IngameMenuVM.Value = new IngameMenuVM(m_CombatIsEnding).AddTo(this);
			m_IngameMenuSettingsButtonVM.Value = new IngameMenuSettingsButtonVM(null).AddTo(this);
			m_PartyVM.Value = new PartyVM(m_CombatIsStarting).AddTo(this);
			m_UnitInfoVM.Value = new UnitInfoVM(Game.Instance.Controllers.PreciseAttackController).AddTo(this);
			m_SignalDeviceVM.Value = new SignalsDeviceVM().AddTo(this);
			m_CombatEndWindowVM.Subscribe(delegate(CombatEndWindowVM vm)
			{
				m_ActionBarVM.Value?.SetForceHidden(vm != null);
			}).AddTo(this);
			UpdateIsTurnBasedActive();
			TurnBasedModeChanged(IsTurnBasedActive.CurrentValue);
			OnUnitChanged();
			m_ComponentsVMsCreated = true;
		}
	}

	private void OnUnitChanged()
	{
		try
		{
			MechanicEntity mechanicEntity = ((TurnController.IsInTurnBasedCombat() && InitiativeTrackerVM.CurrentValue != null && !Game.Instance.Controllers.TurnController.IsPreparationTurn) ? InitiativeTrackerVM.CurrentValue.CurrentUnit.CurrentValue.MechanicEntity : SingleSelectedUnit);
			if (mechanicEntity != m_CurrentUnit.CurrentValue?.MechanicEntity)
			{
				m_CurrentUnit.CurrentValue?.Dispose();
				m_CurrentUnit.Value = ((mechanicEntity != null) ? new CombatMechanicEntityVM(mechanicEntity, m_CombatIsStarting, isCurrent: true) : null);
			}
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"Smth went wrong OnUnitChanged HUDContext: {arg}");
		}
	}

	private void TurnBasedModeChanged(bool state)
	{
		InitiativeTrackerVM.CurrentValue?.Dispose();
		m_TrackerSubscription?.Dispose();
		if (state)
		{
			m_InitiativeTrackerVM.Value = new InitiativeTrackerVM().AddTo(this);
			m_CombatHUDNotificationVM.Value = new CombatHUDNotificationsVM(m_CombatIsStarting);
			m_TrackerSubscription = m_InitiativeTrackerVM.CurrentValue.CurrentUnit.Subscribe(delegate
			{
				OnUnitChanged();
			});
		}
		else
		{
			m_InitiativeTrackerVM.ClearDisposableValue();
			m_CombatHUDNotificationVM.ClearDisposableValue();
			m_TrackerSubscription = null;
		}
	}

	private void ShowCombatStartNotification()
	{
		m_CombatIsStarting.Value = true;
		m_CombatStartNotificationVM.Value = new CombatStartNotificationVM(HideCombatStartNotification);
	}

	private void UpdateIsTurnBasedActive()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			TurnController turnController = Game.Instance.Controllers.TurnController;
			bool turnBasedModeActive = turnController.TurnBasedModeActive;
			if (turnBasedModeActive && Game.Instance.IsPaused)
			{
				Game.Instance.IsPaused = false;
			}
			m_IsTurnBasedActive.Value = turnBasedModeActive;
			m_IsPlayer.Value = !turnBasedModeActive || turnController.IsPlayerTurn || turnController.IsPreparationTurn;
			m_ShowEndTurn.Value = turnController.IsPlayerTurn && turnController.CurrentUnit.IsMyNetRole();
			m_CanEndTurn.Value = turnController.CurrentUnit is BaseUnitEntity baseUnitEntity && baseUnitEntity.CombatState.CanEndTurn();
		}
	}

	private void BeginPreparationTurn(bool canDeploy)
	{
		TryCreateVMs();
		m_CombatStartWindowVM.Value = new CombatStartWindowVM(Game.Instance.Controllers.TurnController.RequestEndPreparationTurn, m_PartyVM, canDeploy).AddTo(this);
		OnUnitChanged();
		m_DeploymentPhase.Value = true;
		m_CanDeploy.Value = canDeploy;
	}

	private void EndPreparationTurn()
	{
		m_CombatStartWindowVM.CurrentValue?.Dispose();
		m_CombatStartWindowVM.Value = null;
		OnUnitChanged();
		m_DeploymentPhase.Value = false;
		m_CanDeploy.Value = false;
	}

	private void HandlePreciseAttackUIShow(bool show)
	{
		m_PreciseAttackVM.CurrentValue?.Dispose();
		m_PreciseAttackVM.Value = (show ? new PreciseAttackVM(Game.Instance.Controllers.PreciseAttackController).AddTo(this) : null);
	}

	private void HideCombatStartNotification()
	{
		if (m_CombatStartNotificationVM.CurrentValue != null)
		{
			m_CombatStartNotificationVM.ClearDisposableValue();
			m_CombatIsStarting.Value = false;
		}
	}

	private void ShowCombatEnd(CombatEndReason reason, Action<bool> closeCallback = null)
	{
		m_CombatIsEnding.Value = true;
		if (reason == CombatEndReason.MoraleVictory)
		{
			m_CombatEndWindowVM.Value = new CombatEndWindowVM(delegate(bool finishCombat)
			{
				m_CombatIsEnding.Value = false;
				m_CombatEndWindowVM.ClearDisposableValue();
				closeCallback?.Invoke(finishCombat);
				m_IsMoraleVictory = finishCombat;
			}, CombatEndReason.MoraleVictory).AddTo(this);
		}
		else
		{
			m_CombatEndWindowVM.Value = new CombatEndWindowVM(delegate
			{
				m_CombatEndWindowVM.ClearDisposableValue();
				m_CombatIsEnding.Value = false;
				closeCallback?.Invoke(obj: true);
			}, CombatEndReason.RegularVictory);
		}
	}
}
