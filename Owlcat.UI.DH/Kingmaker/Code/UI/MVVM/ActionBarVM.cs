using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Morale;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.UI;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarVM : ViewModel, IGameModeHandler, ISubscriber, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IWarhammerAttackHandler, IUnitCommandActHandler, IUnitCommandEndHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IUnitAbilityCooldownHandler, IAbilityExecutionProcessHandler, ILevelUpCompleteUIHandler, ILevelUpManagerUIHandler, IDialogInteractionHandler, IHoverActionBarSlotHandler, IAbilityTargetSelectionUIHandler, IAreaActivationHandler, IUnitDirectHoverUIHandler, IFullScreenUIHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetLobbyPlayersHandler, INetRoleSetHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, ITurnStartHandler, IEntityGainFactHandler, IEntityLostFactHandler, IActionBarSlotsUpdatedHandler
{
	private readonly ReactiveProperty<bool> m_IsForceHidden;

	private readonly ReactiveProperty<bool> m_IsVisible;

	private bool m_IsInFullScreenUI;

	private bool m_TargetSelectionStarted;

	private bool m_SlotsUpdateQueued;

	private bool m_SlotsUpdateQueuedOnTurnStart;

	private IFullScreenUIHandler m_FullScreenUIHandlerImplementation;

	public readonly ActionBarPartConsumablesVM Consumables;

	public readonly ActionBarPartWeaponsVM Weapons;

	public readonly ActionBarPartAbilitiesVM Abilities;

	public readonly VeilThicknessVM VeilThickness;

	public readonly ActionBarMoraleVM Morale;

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentCombatUnit = new ReactiveProperty<CombatMechanicEntityVM>();

	private readonly ReactiveProperty<float> m_CompassAngle = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<string> m_EndTurnText = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsAttackAbilityGroupCooldownAlertActive = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<AbstractUnitEntityView> m_HighlightedUnit = new ReactiveProperty<AbstractUnitEntityView>();

	private readonly ReactiveProperty<ActionBarSlotVM> m_QuickAccessSlot = new ReactiveProperty<ActionBarSlotVM>();

	private readonly ReactiveProperty<bool> m_IsNotControllableCharacter = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_ControllablePlayerNickname = new ReactiveProperty<string>();

	private Action m_DelayedUpdateFunc;

	public ReadOnlyReactiveProperty<CombatMechanicEntityVM> CurrentCombatUnit => m_CurrentCombatUnit;

	public ReadOnlyReactiveProperty<float> CompassAngle => m_CompassAngle;

	public ReadOnlyReactiveProperty<string> EndTurnText => m_EndTurnText;

	public ReadOnlyReactiveProperty<bool> IsAttackAbilityGroupCooldownAlertActive => m_IsAttackAbilityGroupCooldownAlertActive;

	public ReadOnlyReactiveProperty<AbstractUnitEntityView> HighlightedUnit => m_HighlightedUnit;

	public ReadOnlyReactiveProperty<ActionBarSlotVM> QuickAccessSlot => m_QuickAccessSlot;

	public ReadOnlyReactiveProperty<bool> IsNotControllableCharacter => m_IsNotControllableCharacter;

	public ReadOnlyReactiveProperty<string> ControllablePlayerNickname => m_ControllablePlayerNickname;

	private BlueprintAreaPart.LocalMapRotationDegree LocalMapRotation => Game.Instance.CurrentlyLoadedAreaPart.LocalMapRotationDeg;

	private BaseUnitEntity CurrentUnit => CurrentCombatUnit?.CurrentValue?.UnitAsBaseUnitEntity;

	public HUDContext HUDContext => RootVM.Instance.HUDContext;

	private bool IsInGame => IsInGameMode(Game.Instance.CurrentModeType);

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public bool CanSpeedUp => HUDContext.IsTurnBasedActive.CurrentValue;

	public bool CanEndTurn
	{
		get
		{
			if (HUDContext.IsTurnBasedActive.CurrentValue && HUDContext.IsPlayer.CurrentValue)
			{
				return !HUDContext.PartyUnitIsRunningCommand.CurrentValue;
			}
			return false;
		}
	}

	public ReadOnlyReactiveProperty<bool> IsForceHidden => m_IsForceHidden;

	public ActionBarVM(ReactiveProperty<CombatMechanicEntityVM> currentUnit)
	{
		m_IsForceHidden = new ReactiveProperty<bool>().AddTo(this);
		m_IsVisible = new ReactiveProperty<bool>().AddTo(this);
		Consumables = new ActionBarPartConsumablesVM().AddTo(this);
		Weapons = new ActionBarPartWeaponsVM().AddTo(this);
		Abilities = new ActionBarPartAbilitiesVM(isInCharScreen: false).AddTo(this);
		VeilThickness = new VeilThicknessVM().AddTo(this);
		Morale = new ActionBarMoraleVM().AddTo(this);
		m_CurrentCombatUnit = currentUnit;
		CurrentCombatUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}).AddTo(this);
		if (!UtilityGame.IsGlobalMap())
		{
			ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
			{
				UpdateCameraRotation();
			}).AddTo(this);
		}
		EventBus.Subscribe(this).AddTo(this);
		m_DelayedUpdateFunc = DelayedUpdate;
	}

	protected override void OnDispose()
	{
	}

	private void OnUnitChanged()
	{
		UpdateVisibility();
		if (m_IsVisible.Value)
		{
			CurrentUnit.ActionBar.TryToInitialize();
			VeilThickness.Update();
			Consumables.SetUnit(CurrentUnit);
			Weapons.SetUnit(CurrentUnit);
			Abilities.SetUnit(CurrentUnit);
			Morale.SetUnit(CurrentUnit);
			CheckAnotherPlayerTurn();
		}
	}

	private void UpdateVisibility()
	{
		m_IsVisible.Value = CurrentUnit != null && IsInGame && !m_IsInFullScreenUI;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		OnUnitChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void SetSuitableQuickAccessSlot(AbstractUnitEntityView unitEntityView)
	{
		ActionBarSlotVM suitableSlot = GetSuitableSlot(unitEntityView);
		m_QuickAccessSlot.Value = suitableSlot;
	}

	public void ClearQuickAccessSlot()
	{
		m_QuickAccessSlot.Value = null;
	}

	public ReactiveProperty<ActionBarSlotVM> GetQuickAccessSlotVM()
	{
		return m_QuickAccessSlot;
	}

	private void DelayedUpdate()
	{
		Action<IList<ActionBarSlotVM>, bool> action = delegate(IList<ActionBarSlotVM> slots, bool onTurnStart)
		{
			foreach (ActionBarSlotVM slot in slots)
			{
				slot.UpdateResources();
				if (onTurnStart)
				{
					slot.CloseConvertsOnTurnStart();
				}
			}
		};
		action(Consumables.Slots, m_SlotsUpdateQueuedOnTurnStart);
		foreach (ActionBarPartWeaponSetVM set in Weapons.Sets)
		{
			action(set.AllSlots, m_SlotsUpdateQueuedOnTurnStart);
		}
		action(Abilities.Slots, m_SlotsUpdateQueuedOnTurnStart);
		action(Morale.HeroicSlots, m_SlotsUpdateQueuedOnTurnStart);
		action(Morale.BrokenSlots, m_SlotsUpdateQueuedOnTurnStart);
		CheckAnotherPlayerTurn();
		m_SlotsUpdateQueued = false;
	}

	private void UpdateSlots(bool onTurnStart = false)
	{
		if (!m_SlotsUpdateQueued)
		{
			m_SlotsUpdateQueued = true;
			m_SlotsUpdateQueuedOnTurnStart = onTurnStart;
			DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(m_DelayedUpdateFunc);
		}
	}

	private void CheckAnotherPlayerTurn()
	{
		MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
		if (!UtilityNet.InLobbyAndPlaying || currentUnit == null)
		{
			m_IsNotControllableCharacter.Value = false;
			return;
		}
		bool isPlayerFaction = currentUnit.IsPlayerFaction;
		bool flag = ((Game.Instance.CurrentModeType == GameModeType.SpaceCombat) ? (!UtilityNet.IsControlMainCharacter()) : (!currentUnit.IsMyNetRole()));
		m_ControllablePlayerNickname.Value = (PhotonManager.Player.GetNickName(currentUnit.GetPlayer(), out var nickName) ? nickName : string.Empty);
		m_IsNotControllableCharacter.Value = isPlayerFaction && flag;
	}

	private void UpdateCameraRotation()
	{
		if (Application.isPlaying && Game.Instance.CurrentlyLoadedArea != null)
		{
			CameraRig instance = CameraRig.Instance;
			m_CompassAngle.Value = 180f + instance.TargetRotate.y - (float)LocalMapRotation;
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateSlots();
	}

	public void HandleLevelUpComplete()
	{
		OnUnitChanged();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		OnUnitChanged();
	}

	public void HandleUISelectionChanged()
	{
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		m_IsForceHidden.Value = true;
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		m_IsForceHidden.Value = false;
		if (Game.Instance.CurrentlyLoadedArea.IsPartyArea && (dialog.Type == DialogType.Book || dialog.Type == DialogType.Epilog))
		{
			OnUnitChanged();
		}
	}

	public void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
		{
			m_EndTurnText.Value = GetEndTurn(mechanicActionBarSlotAbility.Ability.Blueprint);
		}
	}

	public void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			m_EndTurnText.Value = null;
		}
	}

	public void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
		{
			m_IsAttackAbilityGroupCooldownAlertActive.Value = CheckAbilityHasAttackAbilityGroupCooldown(mechanicActionBarSlotAbility.Ability.Blueprint);
		}
	}

	public void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			m_IsAttackAbilityGroupCooldownAlertActive.Value = false;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_TargetSelectionStarted = true;
		m_IsAttackAbilityGroupCooldownAlertActive.Value = CheckAbilityHasAttackAbilityGroupCooldown(ability.Blueprint);
		m_EndTurnText.Value = GetEndTurn(ability.Blueprint);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_TargetSelectionStarted = false;
		m_IsAttackAbilityGroupCooldownAlertActive.Value = false;
		m_EndTurnText.Value = null;
	}

	private bool CheckAbilityHasAttackAbilityGroupCooldown(BlueprintAbilityWrapper blueprintAbility)
	{
		return blueprintAbility.AbilityGroups.Any((BlueprintAbilityGroup group) => group.NameSafe() == "WeaponAttackAbilityGroup");
	}

	private string GetEndTurn(BlueprintAbilityWrapper blueprintAbility)
	{
		EndTurn component = blueprintAbility.GetComponent<EndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPoints : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private bool IsInGameMode(GameModeType gameMode)
	{
		if (!(gameMode == GameModeType.None) && !(gameMode == GameModeType.Default) && !(gameMode == GameModeType.Pause))
		{
			return gameMode == GameModeType.BugReport;
		}
		return true;
	}

	public void OnAreaActivated()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			OnUnitChanged();
		}
	}

	public void HandleAbilityCooldownStarted(AbilityData ability)
	{
		UpdateSlots();
	}

	public void HandleGroupCooldownRemoved(BlueprintAbilityGroup group)
	{
		UpdateSlots();
	}

	public void HandleCooldownReset()
	{
		UpdateSlots();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		m_HighlightedUnit.Value = (isHover ? unitEntityView : null);
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		bool isInFullScreenUI = m_IsInFullScreenUI;
		m_IsInFullScreenUI = state && fullScreenUIType != FullScreenUIType.Unknown;
		if (isInFullScreenUI != m_IsInFullScreenUI)
		{
			if (!m_IsInFullScreenUI)
			{
				DelayedInvoker.InvokeInFrames(OnUnitChanged, 1);
			}
			else
			{
				OnUnitChanged();
			}
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		UpdateSlots();
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		UpdateSlots();
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		OnUnitChanged();
	}

	public void HandleEndPreparationTurn()
	{
		OnUnitChanged();
		UpdateSlots();
	}

	public void HandleRoleSet(string entityId)
	{
		CheckAnotherPlayerTurn();
		if (!(CurrentUnit?.UniqueId != entityId))
		{
			UpdateSlots();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateSlots();
	}

	public void HandleUnitEndInterruptTurn()
	{
		UpdateSlots();
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		CheckAnotherPlayerTurn();
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		CheckAnotherPlayerTurn();
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateSlots();
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		UpdateSlots();
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		UpdateSlots();
	}

	public void HandleActionBarSlotsUpdated()
	{
		UpdateSlots();
	}

	private ActionBarSlotVM GetSuitableSlot(AbstractUnitEntityView unitEntityView)
	{
		if (unitEntityView.Data.IsPlayerFaction)
		{
			return null;
		}
		List<ActionBarSlotVM> allSlots = Weapons.CurrentSet.CurrentValue.AllSlots;
		_ = Consumables.Slots;
		return allSlots.FirstOrDefault();
	}
}
