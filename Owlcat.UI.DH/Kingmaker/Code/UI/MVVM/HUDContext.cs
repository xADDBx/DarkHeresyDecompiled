using System;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using Kingmaker.Code.UI.MVVM.SkipCutscene;
using Kingmaker.Code.UI.MVVM.UnitInfo;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;
using Kingmaker.Code.View.UI.UIUtilities;
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

	private bool ComponentsVMsCreated;

	private Vector3 m_CachedWorldPosition;

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

	private readonly ReactiveProperty<InitiativeTrackerVM> m_InitiativeTrackerVM;

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentUnit;

	private readonly ReactiveProperty<CombatStartWindowVM> m_CombatStartWindowVM;

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

	public ReadOnlyReactiveProperty<InspectVM> InspectVM => m_InspectVM;

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

	public HUDContext(ReactiveProperty<CombatStartWindowVM> combatStartWindowVM, ReactiveProperty<InitiativeTrackerVM> initiativeTrackerVM, ReactiveProperty<CombatMechanicEntityVM> currentUnit, ReactiveProperty<ActionBarVM> actionBarVM, ReactiveProperty<InspectVM> inspectVM, ReactiveProperty<CombatLogVM> combatLogVM, ReactiveProperty<IngameMenuVM> ingameMenuVM, ReactiveProperty<IngameMenuSettingsButtonVM> ingameMenuSettingsButtonVM, ReactiveProperty<PartyVM> partyVM, ReactiveProperty<PreciseAttackVM> preciseAttackVM, ReactiveProperty<UnitInfoVM> unitInfoVM, ReactiveProperty<SignalsDeviceVM> signalDeviceVM, ReactiveProperty<SkipCutsceneVM> skipCutsceneVM, ReactiveProperty<CombatEndWindowVM> combatEndWindowVM)
	{
		m_CombatStartWindowVM = combatStartWindowVM;
		m_InitiativeTrackerVM = initiativeTrackerVM;
		m_CurrentUnit = currentUnit;
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
			HandleBeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateIsTurnBasedActive();
		}).AddTo(this);
		IsTurnBasedActive.Subscribe(delegate(bool s)
		{
			TurnBasedModeChanged(s);
		}).AddTo(this);
		Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}).AddTo(this);
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
		Game.Instance.Controllers.PreciseAttackController.Show.Subscribe(HandlePreciseAttackUIShow).AddTo(this);
		TryCreateVMs();
	}

	private void OnUnitChanged()
	{
		try
		{
			MechanicEntity mechanicEntity = ((TurnController.IsInTurnBasedCombat() && InitiativeTrackerVM.CurrentValue != null && !Game.Instance.Controllers.TurnController.IsPreparationTurn) ? InitiativeTrackerVM.CurrentValue.CurrentUnit.CurrentValue.MechanicEntity : SingleSelectedUnit);
			if (mechanicEntity != m_CurrentUnit.CurrentValue?.MechanicEntity)
			{
				m_CurrentUnit.CurrentValue?.Dispose();
				m_CurrentUnit.Value = ((mechanicEntity != null) ? new CombatMechanicEntityVM(mechanicEntity, isCurrent: true) : null);
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Error("Smth went wrong OnUnitChanged HUDContext: " + ex.Message);
		}
	}

	private void TurnBasedModeChanged(bool state)
	{
		InitiativeTrackerVM.CurrentValue?.Dispose();
		m_TrackerSubscription?.Dispose();
		if (state)
		{
			m_InitiativeTrackerVM.Value = new InitiativeTrackerVM().AddTo(this);
			m_TrackerSubscription = InitiativeTrackerVM.CurrentValue.CurrentUnit.Subscribe(delegate
			{
				OnUnitChanged();
			});
		}
		else
		{
			m_InitiativeTrackerVM.Value = null;
			m_TrackerSubscription = null;
		}
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
		HandleEndPreparationTurn();
		m_CurrentUnit.CurrentValue?.Dispose();
		m_CurrentUnit.Value = null;
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

	public void OnAreaActivated()
	{
		TryCreateVMs();
	}

	private void TryCreateVMs()
	{
		if (Game.Instance.LoadedArea == null)
		{
			DisposeAll();
		}
		else if (!ComponentsVMsCreated)
		{
			m_ActionBarVM.Value = new ActionBarVM(m_CurrentUnit).AddTo(this);
			m_InspectVM.Value = new InGameInspectVM().AddTo(this);
			m_CombatLogVM.Value = new CombatLogVM().AddTo(this);
			m_IngameMenuVM.Value = new IngameMenuVM().AddTo(this);
			m_IngameMenuSettingsButtonVM.Value = new IngameMenuSettingsButtonVM().AddTo(this);
			m_PartyVM.Value = new PartyVM().AddTo(this);
			m_UnitInfoVM.Value = new UnitInfoVM(Game.Instance.Controllers.PreciseAttackController).AddTo(this);
			m_SignalDeviceVM.Value = new SignalsDeviceVM().AddTo(this);
			UpdateIsTurnBasedActive();
			TurnBasedModeChanged(IsTurnBasedActive.CurrentValue);
			OnUnitChanged();
			ComponentsVMsCreated = true;
		}
	}

	public void OnAreaBeginUnloading()
	{
		DisposeAll();
	}

	private void DisposeAll()
	{
		ActionBarVM.CurrentValue?.Dispose();
		InspectVM.CurrentValue?.Dispose();
		m_CombatLogVM.CurrentValue?.Dispose();
		m_IngameMenuVM.CurrentValue?.Dispose();
		m_IngameMenuSettingsButtonVM.CurrentValue?.Dispose();
		m_PartyVM.CurrentValue?.Dispose();
		m_UnitInfoVM.CurrentValue?.Dispose();
		m_SignalDeviceVM.CurrentValue?.Dispose();
		m_ActionBarVM.Value = null;
		m_InspectVM.Value = null;
		m_CombatLogVM.Value = null;
		m_IngameMenuVM.Value = null;
		m_IngameMenuSettingsButtonVM.Value = null;
		m_PartyVM.Value = null;
		m_UnitInfoVM.Value = null;
		m_SignalDeviceVM.Value = null;
		ComponentsVMsCreated = false;
	}

	public void OnAreaDidLoad()
	{
	}

	private void HandlePreciseAttackUIShow(bool show)
	{
		m_PreciseAttackVM.CurrentValue?.Dispose();
		m_PreciseAttackVM.Value = (show ? new PreciseAttackVM(Game.Instance.Controllers.PreciseAttackController).AddTo(this) : null);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		TryCreateVMs();
		m_CombatStartWindowVM.Value = new CombatStartWindowVM(Game.Instance.Controllers.TurnController.RequestEndPreparationTurn, m_PartyVM, canDeploy).AddTo(this);
		OnUnitChanged();
		m_DeploymentPhase.Value = true;
		m_CanDeploy.Value = canDeploy;
	}

	public void HandleEndPreparationTurn()
	{
		m_CombatStartWindowVM.CurrentValue?.Dispose();
		m_CombatStartWindowVM.Value = null;
		OnUnitChanged();
		m_DeploymentPhase.Value = false;
		m_CanDeploy.Value = false;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			HandleEndPreparationTurn();
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_SkipCutsceneVM.Value = new SkipCutsceneVM().AddTo(this);
			HandleEndPreparationTurn();
		}
		OnUnitChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_SkipCutsceneVM.CurrentValue?.Dispose();
			m_SkipCutsceneVM.Value = null;
			if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
			{
				HandleBeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
			}
		}
		OnUnitChanged();
	}

	public void OpenNetRoles()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandleRoleSet(string entityId)
	{
		m_PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	public void HandleTransferProgressChanged(bool value)
	{
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandleNLoadingScreenClosed()
	{
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		m_CombatEndWindowVM.Value = new CombatEndWindowVM(delegate(bool i)
		{
			OnCloseCombatEnd(i, callback);
		}, CombatEndReason.MoraleVictory).AddTo(this);
	}

	private void OnCloseCombatEnd(bool endCombat, IMoraleVictoryConfirmationRequest.Callback callback)
	{
		CombatEndWindowVM.CurrentValue?.Dispose();
		m_CombatEndWindowVM.Value = null;
		callback(endCombat);
	}

	public void HandleHighlightChange(bool isOn)
	{
		m_ForceHotKeyPressed.Value = isOn;
		m_CachedWorldPosition = Game.Instance.Controllers.ClickEventsController.WorldPosition;
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor.IsInPlayerParty && (command.Executor == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity || command.Target == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity))
		{
			m_ReleasePartyUnitCommand?.Dispose();
			m_PartyUnitIsRunningCommand.Value = true;
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor.IsInPlayerParty && (command.Executor == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity || command.Target == m_CurrentUnit.CurrentValue?.UnitAsBaseUnitEntity))
		{
			m_ReleasePartyUnitCommand = ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(2), delegate
			{
				m_PartyUnitIsRunningCommand.Value = false;
			}).AddTo(this);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity == null || !mechanicEntity.IsInPlayerParty)
		{
			m_ReleasePartyUnitCommand?.Dispose();
			m_PartyUnitIsRunningCommand.Value = false;
		}
	}
}
