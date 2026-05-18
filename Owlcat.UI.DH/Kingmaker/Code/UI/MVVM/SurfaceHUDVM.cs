using System;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using Kingmaker.Code.UI.MVVM.UnitInfo;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

[Obsolete("Exists until SurfaceHUDConsoleView is refactored.")]
public class SurfaceHUDVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAreaActivationHandler, ISubscriber, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IGameModeHandler, INetRoleSetHandler, INetEvents
{
	private readonly ReactiveProperty<bool> m_IsTurnBasedActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowEndTurn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanEndTurn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_DeploymentPhase = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_PlayerHaveRoles = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NetFirstLoadState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<InitiativeTrackerVM> m_InitiativeTrackerVM = new ReactiveProperty<InitiativeTrackerVM>();

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentUnit = new ReactiveProperty<CombatMechanicEntityVM>();

	private readonly ReactiveProperty<CombatStartWindowVM> m_PreparationTurnWindowVM = new ReactiveProperty<CombatStartWindowVM>();

	private IDisposable m_TrackerSubscription;

	public readonly InspectVM InspectVM;

	public readonly CombatLogVM CombatLogVM;

	public readonly IngameMenuVM IngameMenuVM;

	public readonly PartyVM PartyVM;

	public readonly ActionBarVM ActionBarVM;

	private BaseUnitEntity SingleSelectedUnit => Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Value;

	public ReadOnlyReactiveProperty<bool> IsTurnBasedActive => m_IsTurnBasedActive;

	public ReadOnlyReactiveProperty<bool> ShowEndTurn => m_ShowEndTurn;

	public ReadOnlyReactiveProperty<bool> CanEndTurn => m_CanEndTurn;

	public ReadOnlyReactiveProperty<bool> DeploymentPhase => m_DeploymentPhase;

	public ReadOnlyReactiveProperty<bool> PlayerHaveRoles => m_PlayerHaveRoles;

	public ReadOnlyReactiveProperty<bool> NetFirstLoadState => m_NetFirstLoadState;

	public ReadOnlyReactiveProperty<InitiativeTrackerVM> InitiativeTrackerVM => m_InitiativeTrackerVM;

	public ReadOnlyReactiveProperty<CombatMechanicEntityVM> CurrentUnit => m_CurrentUnit;

	public ReadOnlyReactiveProperty<CombatStartWindowVM> PreparationTurnWindowVM => m_PreparationTurnWindowVM;

	public SurfaceHUDVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		UpdateIsTurnBasedActive();
		TurnBasedModeChanged(IsTurnBasedActive.CurrentValue);
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
		}
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			UpdateIsTurnBasedActive();
		}));
		AddDisposable(IsTurnBasedActive.Subscribe(delegate(bool s)
		{
			TurnBasedModeChanged(s);
		}));
		AddDisposable(IngameMenuVM = new IngameMenuVM(null));
		AddDisposable(new IngameMenuSettingsButtonVM(null));
		AddDisposable(InspectVM = new InGameInspectVM());
		AddDisposable(CombatLogVM = new CombatLogVM());
		AddDisposable(PartyVM = new PartyVM());
		AddDisposable(ActionBarVM = new ActionBarVM(m_CurrentUnit, null));
		AddDisposable(new SignalsDeviceVM());
		AddDisposable(new UnitInfoVM(Game.Instance.Controllers.PreciseAttackController));
		AddDisposable(Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		m_NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	private void OnUnitChanged()
	{
		MechanicEntity mechanicEntity = ((TurnController.IsInTurnBasedCombat() && InitiativeTrackerVM.CurrentValue != null && !Game.Instance.Controllers.TurnController.IsPreparationTurn) ? InitiativeTrackerVM.CurrentValue.CurrentUnit.CurrentValue.MechanicEntity : SingleSelectedUnit);
		if (mechanicEntity != CurrentUnit.CurrentValue?.MechanicEntity)
		{
			CurrentUnit.CurrentValue?.Dispose();
			m_CurrentUnit.Value = ((mechanicEntity != null) ? new CombatMechanicEntityVM(mechanicEntity, null, isCurrent: true) : null);
		}
	}

	private void TurnBasedModeChanged(bool state)
	{
		InitiativeTrackerVM.CurrentValue?.Dispose();
		m_TrackerSubscription?.Dispose();
		if (state)
		{
			InitiativeTrackerVM disposable = (m_InitiativeTrackerVM.Value = new InitiativeTrackerVM());
			AddDisposable(disposable);
			m_TrackerSubscription = InitiativeTrackerVM.CurrentValue?.CurrentUnit.Subscribe(delegate
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

	protected override void DisposeImplementation()
	{
		HandleEndPreparationTurn();
		CurrentUnit.CurrentValue?.Dispose();
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
			m_ShowEndTurn.Value = turnController.IsPlayerTurn && turnController.CurrentUnit.IsMyNetRole();
			m_CanEndTurn.Value = turnController.CurrentUnit is BaseUnitEntity baseUnitEntity && baseUnitEntity.CombatState.CanEndTurn();
		}
	}

	public void OnAreaActivated()
	{
		UpdateIsTurnBasedActive();
		TurnBasedModeChanged(IsTurnBasedActive.CurrentValue);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
	}

	public void HandleEndPreparationTurn()
	{
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
			HandleEndPreparationTurn();
		}
		OnUnitChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene && Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.Controllers.TurnController.IsDeploymentAllowed);
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
}
