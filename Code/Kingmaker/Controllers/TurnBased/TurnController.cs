using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.PubSubSystem;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.Signals;
using Kingmaker.UI.AR;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class TurnController : IControllerEnable, IController, IControllerDisable, IControllerStart, IControllerStop, IControllerTick, IAreaHandler, ISubscriber, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ICameraFocusEventHandler
{
	private static class InterruptTurnNotifier
	{
		public static InterruptionData interruptionData;

		public static void Execute(IInterruptTurnStartHandler h)
		{
			h.HandleUnitStartInterruptTurn(interruptionData);
		}
	}

	private static class TurnPreStartNotifier
	{
		public static bool isTurnBased;

		public static void Execute(ITurnPreStartHandler h)
		{
			h.HandleUnitPreStartTurn(isTurnBased);
		}
	}

	private static class TurnStartNotifier
	{
		public static bool isTurnBased;

		public static void Execute(ITurnStartHandler h)
		{
			h.HandleUnitStartTurn(isTurnBased);
		}
	}

	private static class InterruptTurnEndNotifier
	{
		public static void Execute(IInterruptTurnEndHandler h)
		{
			h.HandleUnitEndInterruptTurn();
		}
	}

	private static class TurnEndNotifier
	{
		public static bool isTurnBased;

		public static void Execute(ITurnEndHandler h)
		{
			h.HandleUnitEndTurn(isTurnBased);
		}
	}

	private static class RoundEndNotifier
	{
		public static bool isTurnBased;

		public static void Execute(IRoundEndHandler h)
		{
			h.HandleRoundEnd(isTurnBased);
		}
	}

	private static class RoundStartNotifier
	{
		public static bool isTurnBased;

		public static void Execute(IRoundStartHandler h)
		{
			h.HandleRoundStart(isTurnBased);
		}
	}

	public sealed class InterruptTurnEndMark : ContextFlag<InterruptTurnEndMark>
	{
	}

	private const float OutOfCombatSpeedMod = 5f;

	public const int FirstCombatRound = 1;

	public const int DeploymentEnemyRestrictionRadius = 1;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("TurnController");

	private static readonly TimeSpan OutOfCombatRoundDuration = 5f.Seconds();

	private readonly HashSet<MechanicEntity> m_WaitingForInitiativeEntities = new HashSet<MechanicEntity>();

	private readonly List<MechanicEntity> m_JoinedThisTickEntities = new List<MechanicEntity>();

	private bool m_IsControllerStarted;

	private bool m_IsControllerActive;

	private List<UnitInterruptTurnParams> m_DelayedInterrupts = new List<UnitInterruptTurnParams>();

	private UnitInterruptTurnParams m_ScheduledInterrupt;

	private TimeSpan? m_CustomEventEndTime;

	private TimeSpan? m_UnableToActUnitForceTurnEndTime;

	private TimeSpan? m_ChannelingLogicInterruptTurnEndTime;

	private SignalWrapper m_StartBattleSignal;

	private int m_DeploymentDiagnosticsSessionId;

	private TurnDataPart m_Data;

	private bool m_OnAreaBeginUnloading;

	private bool m_IsFirstTick;

	private Action<IInterruptTurnStartHandler> m_InterruptTurnNotifierAction = InterruptTurnNotifier.Execute;

	private Action<ITurnPreStartHandler> m_TurnPreStartNotifierAction = TurnPreStartNotifier.Execute;

	private Action<ITurnStartHandler> m_TurnStartNotifierAction = TurnStartNotifier.Execute;

	private Action<IInterruptTurnEndHandler> m_InterruptTurnEndNotifierAction = InterruptTurnEndNotifier.Execute;

	private Action<ITurnEndHandler> m_TurnEndNotifierAction = TurnEndNotifier.Execute;

	private Action<IRoundEndHandler> m_RoundEndNotifierAction = RoundEndNotifier.Execute;

	private Action<IRoundStartHandler> m_RoundStartNotifierAction = RoundStartNotifier.Execute;

	public bool TurnBasedModeActive
	{
		get
		{
			if (m_IsControllerStarted)
			{
				return InCombat;
			}
			return false;
		}
	}

	public bool InCombat => MaybeData?.InCombat ?? false;

	public bool IsPlayerTurn
	{
		get
		{
			if (m_IsControllerStarted)
			{
				MechanicEntity currentUnit = CurrentUnit;
				if (currentUnit != null && currentUnit.IsInPlayerParty)
				{
					return !IsRoamingTurn;
				}
			}
			return false;
		}
	}

	public bool IsAiTurn
	{
		get
		{
			if (m_IsControllerStarted)
			{
				MechanicEntity currentUnit = CurrentUnit;
				if (currentUnit != null && !currentUnit.IsInPlayerParty)
				{
					return !IsRoamingTurn;
				}
			}
			return false;
		}
	}

	public bool IsUltimateAbilityUsedThisRound
	{
		get
		{
			return MaybeData?.IsUltimateAbilityUsedThisRound ?? false;
		}
		private set
		{
			Data.IsUltimateAbilityUsedThisRound = value;
		}
	}

	public TurnOrderQueue TurnOrder => MaybeData?.TurnOrder ?? throw new InvalidOperationException("TurnController didn't start yet");

	public TurnOrderQueue MaybeTurnOrder => MaybeData?.TurnOrder;

	public IEnumerable<MechanicEntity> CurrentRoundUnitsOrder
	{
		get
		{
			if (m_IsControllerStarted)
			{
				TurnOrderQueue maybeTurnOrder = MaybeTurnOrder;
				if (maybeTurnOrder != null)
				{
					return maybeTurnOrder.CurrentRoundUnitsOrder;
				}
			}
			return Array.Empty<MechanicEntity>();
		}
	}

	public IEnumerable<MechanicEntity> NextRoundUnitsOrder
	{
		get
		{
			if (m_IsControllerStarted)
			{
				TurnOrderQueue maybeTurnOrder = MaybeTurnOrder;
				if (maybeTurnOrder != null)
				{
					return maybeTurnOrder.NextRoundUnitsOrder;
				}
			}
			return Array.Empty<MechanicEntity>();
		}
	}

	public bool IsRoamingTurn
	{
		get
		{
			TurnOrderQueue maybeTurnOrder = MaybeTurnOrder;
			if (maybeTurnOrder != null)
			{
				return maybeTurnOrder.CurrentTurnType == CombatTurnType.Roaming;
			}
			return false;
		}
	}

	public bool IsManualCombatTurn
	{
		get
		{
			TurnOrderQueue maybeTurnOrder = MaybeTurnOrder;
			if (maybeTurnOrder != null)
			{
				return maybeTurnOrder.CurrentTurnType == CombatTurnType.ManualCombat;
			}
			return false;
		}
	}

	public bool IsPreparationTurn
	{
		get
		{
			TurnOrderQueue maybeTurnOrder = MaybeTurnOrder;
			if (maybeTurnOrder != null)
			{
				return maybeTurnOrder.CurrentTurnType == CombatTurnType.Preparation;
			}
			return false;
		}
	}

	public int DeploymentDiagnosticsSessionId => m_DeploymentDiagnosticsSessionId;

	public bool IsCombatLockActive
	{
		get
		{
			if (TurnBasedModeActive)
			{
				return !IsPreparationTurn;
			}
			return false;
		}
	}

	public bool NeedDeploymentPhase => Game.Instance.CurrentModeType != GameModeType.SpaceCombat;

	public bool IsDeploymentAllowed
	{
		get
		{
			ActiveEncounter current = ActiveEncounter.Current;
			if (current != null)
			{
				return !current.IsPartyAmbushed;
			}
			return false;
		}
	}

	public IEnumerable<MechanicEntity> AllUnits
	{
		get
		{
			foreach (MechanicEntity combatParticipant2 in Game.Instance.EntityPools.CombatParticipants)
			{
				if (combatParticipant2 is ICombatParticipant { Active: not false })
				{
					yield return combatParticipant2;
				}
			}
		}
	}

	public IEnumerable<MechanicEntity> EntitiesInCombat => AllUnits.Where((MechanicEntity i) => i.IsInCombat);

	public IEnumerable<MechanicEntity> UnitsInCombat => AllUnits.Where((MechanicEntity i) => i.IsInCombat && i is BaseUnitEntity);

	[CanBeNull]
	public MechanicEntity CurrentUnit => MaybeTurnOrder?.CurrentUnit;

	public bool IsChannelingLogicInterruptTurn { get; private set; }

	public int CombatRound
	{
		get
		{
			if (!m_IsControllerStarted)
			{
				return 0;
			}
			return MaybeData?.CombatRound ?? 0;
		}
	}

	public int GameRound => MaybeData?.GameRound ?? 0;

	public bool EndTurnRequested => MaybeData?.EndTurnRequested ?? false;

	[NotNull]
	private TurnDataPart Data => m_Data ?? throw new InvalidOperationException("TurnController didn't start yet");

	[CanBeNull]
	private TurnDataPart MaybeData => m_Data;

	private bool CanEndTurn => CurrentUnit.GetCombatStateOptional()?.CanEndTurn() ?? true;

	public IEnumerable<UnitSquad> UnitSquads => CurrentRoundUnitsOrder.OfType<UnitSquad>();

	public bool IsSpaceCombat => Game.Instance.CurrentModeType == GameModeType.SpaceCombat;

	public bool IsPreciseAttack => Game.Instance.Controllers.PreciseAttackController.HasTarget;

	public void OnStart()
	{
		m_IsControllerStarted = true;
		m_IsFirstTick = true;
		m_WaitingForInitiativeEntities.Clear();
		m_Data = Game.Instance.Player.GetOrCreate<TurnDataPart>();
		if (TurnBasedModeActive)
		{
			TurnOrder.RestoreCurrentUnit();
		}
		if (Data.InCombat && Game.Instance.Controllers.TurnController.IsPreparationTurn && NeedDeploymentPhase)
		{
			BeginPreparationTurn(IsDeploymentAllowed, raiseEvent: false);
		}
		EventBus.RaiseEvent(delegate(ITurnBasedModeStartHandler h)
		{
			h.HandleTurnBasedModeStarted();
		});
	}

	public void OnEnable()
	{
		if (Data.InCombat)
		{
			EventBus.RaiseEvent(delegate(ITurnBasedModeResumeHandler h)
			{
				h.HandleTurnBasedModeResumed();
			});
		}
	}

	public void OnDisable()
	{
		ActiveEncounter.Current?.TryCompleteImmediately();
		if (Data.InCombat && (CurrentUnit == null || Data.EndTurnRequested) && !m_OnAreaBeginUnloading)
		{
			if (IsRoamingTurn)
			{
				EndRoamingUnitsTurn();
			}
			EndUnitTurn(CurrentUnit, isTurnBased: true, Data.EndTurnRequested);
		}
		Game.Instance.Player.UISettings.StopSpeedUp();
	}

	public void OnStop()
	{
		ExitTb();
		Data.LastTurnTime = TimeSpan.Zero;
		m_IsControllerStarted = false;
		m_OnAreaBeginUnloading = false;
		m_Data = null;
	}

	public bool CanFinishDeploymentPhase()
	{
		if (!IsDeploymentAllowed)
		{
			return true;
		}
		BaseUnitEntity[] array = Game.Instance.EntityPools.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy).ToArray();
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.CombatState.StartedCombatNearEnemy || (bool)item.Features.CanDeployNearEnemies)
			{
				continue;
			}
			BaseUnitEntity[] array2 = array;
			foreach (BaseUnitEntity enemy in array2)
			{
				if (!item.IsDeadOrUnconscious && IsInDeploymentRestrictionZone(item, enemy))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsInDeploymentRestrictionZone(BaseUnitEntity unit, BaseUnitEntity enemy)
	{
		if (unit.DistanceToInCells(enemy) > 1)
		{
			return false;
		}
		return !IsSeparatedByDeploymentObstacle(enemy, unit.Position);
	}

	public static bool IsInDeploymentRestrictionZone(Vector3 unitPosition, BaseUnitEntity enemy)
	{
		if (enemy.DistanceToInCells(unitPosition) > 1)
		{
			return false;
		}
		return !IsSeparatedByDeploymentObstacle(enemy, unitPosition);
	}

	private static bool IsSeparatedByDeploymentObstacle(BaseUnitEntity enemy, Vector3 position)
	{
		GridNode currentUnwalkableNode = enemy.CurrentUnwalkableNode;
		Linecast.HasConnectionTransition condition = Linecast.HasConnectionTransition.Instance;
		Linecast.GraphHitInfo hit;
		return Linecast.LinecastGrid(currentUnwalkableNode.Graph, enemy.Position, position, currentUnwalkableNode, out hit, ref condition);
	}

	private void EnterTb()
	{
		if (Data.InCombat)
		{
			return;
		}
		Data.InCombat = true;
		EventBus.RaiseEvent(delegate(ITurnBasedModeHandler h)
		{
			h.HandleTurnBasedModeSwitched(isTurnBased: true);
		});
		InitiativeHelper.Update();
		AddUnitsToCombat();
		NetService.Instance.CancelCurrentCommands();
		if (!Game.Instance.Controllers.AbilityExecutor.Abilities.Empty())
		{
			Game.Instance.Controllers.AbilityExecutor.Abilities.ForEach(delegate(AbilityExecutionProcess ability)
			{
				ability.InstantDeliver();
			});
		}
		NextRound(isFirst: true);
		if (NeedDeploymentPhase)
		{
			BeginPreparationTurn(IsDeploymentAllowed);
		}
		else
		{
			NextTurnTB();
		}
		HandleJoinedThisTickEntities();
	}

	public void ExitTb()
	{
		if (!Data.InCombat)
		{
			return;
		}
		Data.InCombat = false;
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
		{
			allBaseAwakeUnit.Commands.InterruptAllInterruptible();
		}
		EventBus.RaiseEvent(delegate(ITurnBasedModeHandler h)
		{
			h.HandleTurnBasedModeSwitched(isTurnBased: false);
		});
		Data.CombatRound = 0;
		TurnOrder.Clear();
		Data.EndTurnRequested = false;
		m_UnableToActUnitForceTurnEndTime = null;
		m_CustomEventEndTime = null;
		Game.Instance.Player.UISettings.StopSpeedUp();
		InitiativeHelper.Update();
	}

	private void HandleJoinedThisTickEntities()
	{
		foreach (MechanicEntity joinedThisTickEntity in m_JoinedThisTickEntities)
		{
			PrepareUnitForNewTurn(joinedThisTickEntity, isTurnBased: true);
			EventBus.RaiseEvent((IMechanicEntity)joinedThisTickEntity, (Action<IEntityJoinTBCombat>)delegate(IEntityJoinTBCombat h)
			{
				h.HandleEntityJoinTBCombat();
			}, isCheckRuntime: true);
		}
		m_JoinedThisTickEntities.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		bool flag = IsInTurnBasedCombat();
		if (!TurnBasedModeActive && flag)
		{
			EnterTb();
		}
		if (m_IsFirstTick)
		{
			ApplyPostLoadFixes();
			m_IsFirstTick = false;
		}
		SetTime();
		if (!Data.InCombat)
		{
			TickRoundRT();
			return;
		}
		TryInterruptTurn();
		TryRollInitiative();
		TrySelectCurrentUnitInUI();
		HandleCurrentUnitUnableToAct();
		HandleCurrentUnitChannelingLogicInterruptTurn();
		bool flag2 = m_CustomEventEndTime.HasValue && m_CustomEventEndTime > Game.Instance.Controllers.TimeController.GameTime;
		if (CurrentUnit == null || (Data.EndTurnRequested && !flag2))
		{
			EndUnitTurn(CurrentUnit, isTurnBased: true, Data.EndTurnRequested);
			m_CustomEventEndTime = null;
		}
		HandleJoinedThisTickEntities();
		if (!m_StartBattleSignal.IsEmpty && SignalService.Instance.CheckReady(ref m_StartBattleSignal))
		{
			ForceEndPreparationTurn();
		}
	}

	private void TickRoundRT()
	{
		if (Game.Instance.Player.GameTime - Data.LastTurnTime > OutOfCombatRoundDuration)
		{
			Data.LastTurnTime = Game.Instance.Player.GameTime;
			NextRoundRT();
		}
	}

	private void TryRollInitiative()
	{
		m_WaitingForInitiativeEntities.AddRange(EntitiesInCombat.Where((MechanicEntity unit) => unit.Initiative.Empty));
		if (m_WaitingForInitiativeEntities.Empty())
		{
			return;
		}
		try
		{
			bool relax = !IsSpaceCombat && EntitiesInCombat.All((MechanicEntity i) => i.Initiative.Empty);
			InitiativeHelper.Roll(m_WaitingForInitiativeEntities, relax);
		}
		finally
		{
			m_JoinedThisTickEntities.AddRange(m_WaitingForInitiativeEntities);
			m_WaitingForInitiativeEntities.Clear();
		}
	}

	private void ScheduleInitiativeRoll(BaseUnitEntity unit)
	{
		PartSquad squadOptional = unit.GetSquadOptional();
		if (squadOptional != null)
		{
			UnitSquad squad = squadOptional.Squad;
			if (squad != null && squad.Initiative.Empty)
			{
				m_WaitingForInitiativeEntities.Add(squad);
			}
		}
		if (unit.Initiative.Empty)
		{
			m_WaitingForInitiativeEntities.Add(unit);
		}
	}

	private void TrySelectCurrentUnitInUI()
	{
		if (!IsPreparationTurn && CurrentUnit.IsDirectlyControllable() && CurrentUnit is BaseUnitEntity unit && (Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Count != 1 || Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit != CurrentUnit))
		{
			Game.Instance.Controllers.SelectionCharacter.SetSelected(unit, force: true);
		}
	}

	public void TryEndPlayerTurnManually()
	{
		if (!IsPlayerTurn)
		{
			return;
		}
		if (CanEndTurn)
		{
			if (CurrentUnit.IsMyNetRole())
			{
				CombatSounds.Instance.Combat.EndTurn.Play();
				Game.Instance.GameCommandQueue.EndTurnManually(CurrentUnit);
			}
		}
		else
		{
			ShowShouldFlyFurtherMessage();
		}
	}

	public void TryEndPlayerTurn(EntityRef<MechanicEntity> mechanicEntity)
	{
		if (IsPlayerTurn && mechanicEntity != CurrentUnit)
		{
			Logger.Warning("Attempt to end the turn of another unit! Current=" + CurrentUnit?.UniqueId + ", expected=" + mechanicEntity.Id);
		}
		else
		{
			TryEndPlayerTurn();
		}
	}

	private void TryEndPlayerTurn()
	{
		if (IsPlayerTurn && CanEndTurn)
		{
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
			RequestEndTurn();
		}
		else
		{
			ShowShouldFlyFurtherMessage();
		}
	}

	private void ShowShouldFlyFurtherMessage()
	{
		LocalizedString restrictionText = LocalizedTexts.Instance.Reasons.ShouldFlyFurtherToEndTurn;
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(restrictionText, addToLog: false);
		});
	}

	public void RequestEndTurn()
	{
		Data.EndTurnRequested = TurnBasedModeActive;
		m_UnableToActUnitForceTurnEndTime = null;
		m_ChannelingLogicInterruptTurnEndTime = null;
	}

	public void EndRoamingUnitsTurn()
	{
		TurnOrder.EndRoamingUnitsTurn();
	}

	private void SetTime()
	{
		float playerTimeScale = 1f;
		if (IsRoamingTurn)
		{
			playerTimeScale = 5f;
		}
		else if (IsAiTurn)
		{
			MechanicEntity currentUnit = CurrentUnit;
			playerTimeScale = ((currentUnit != null && currentUnit.IsInFogOfWar) ? 16f : 1f);
		}
		Game.Instance.Controllers.TimeController.PlayerTimeScale = playerTimeScale;
	}

	private void NextRound(bool isFirst)
	{
		RoundEndNotifier.isTurnBased = TurnBasedModeActive;
		EventBus.RaiseEvent(m_RoundEndNotifierAction);
		if (TurnBasedModeActive)
		{
			Data.CombatRound = (isFirst ? 1 : (Data.CombatRound + 1));
			IsUltimateAbilityUsedThisRound = false;
		}
		Data.GameRound++;
		RoundStartNotifier.isTurnBased = TurnBasedModeActive;
		EventBus.RaiseEvent(m_RoundStartNotifierAction);
	}

	private void NextRoundRT()
	{
		List<MechanicEntity> list = ListPool<MechanicEntity>.Claim();
		try
		{
			foreach (MechanicEntity combatParticipant2 in Game.Instance.EntityPools.CombatParticipants)
			{
				if (combatParticipant2 is ICombatParticipant { Active: not false })
				{
					list.Add(combatParticipant2);
				}
			}
			foreach (MechanicEntity item in list)
			{
				if (!(item is UnitEntity { IsExtra: not false }))
				{
					EndUnitTurn(item, isTurnBased: false);
				}
			}
			NextRound(isFirst: false);
			foreach (MechanicEntity item2 in list)
			{
				if (!(item2 is UnitEntity { IsExtra: not false }))
				{
					StartUnitTurn(item2, isTurnBased: false);
				}
			}
		}
		finally
		{
			ListPool<MechanicEntity>.Release(list);
		}
	}

	private void StartUnitTurn([NotNull] MechanicEntity entity, bool isTurnBased, InterruptionData interruptionData = null)
	{
		IsChannelingLogicInterruptTurn = ContextData<InterruptTurnData>.Current?.Source is ChannelingLogic.InitiativeHolder;
		TurnPreStartNotifier.isTurnBased = isTurnBased;
		EventBus.RaiseEvent((IMechanicEntity)entity, m_TurnPreStartNotifierAction, isCheckRuntime: true);
		if (entity is UnitSquad { Units: var units })
		{
			for (int i = 0; i < units.Count; i++)
			{
				StartUnitTurnInternal(units[i].ToBaseUnitEntity(), isTurnBased);
			}
		}
		else if (entity is ChannelingLogic.InitiativeHolder initiativeHolder)
		{
			HandleChannelingInterruptTurn(initiativeHolder, isTurnBased);
		}
		else if (!entity.IsInSquad)
		{
			StartUnitTurnInternal(entity, isTurnBased);
		}
		if (entity.Initiative.InterruptingOrder > 0)
		{
			InterruptTurnNotifier.interruptionData = interruptionData;
			EventBus.RaiseEvent((IMechanicEntity)entity, m_InterruptTurnNotifierAction, isCheckRuntime: true);
		}
		else
		{
			TurnStartNotifier.isTurnBased = isTurnBased;
			EventBus.RaiseEvent((IMechanicEntity)entity, m_TurnStartNotifierAction, isCheckRuntime: true);
		}
		if (m_DelayedInterrupts.Empty())
		{
			return;
		}
		PartUnitCommands commandsOptional = CurrentUnit.GetCommandsOptional();
		if (commandsOptional == null)
		{
			return;
		}
		foreach (UnitInterruptTurnParams delayedInterrupt in m_DelayedInterrupts)
		{
			commandsOptional.AddToQueue(delayedInterrupt);
		}
		m_DelayedInterrupts.Clear();
	}

	private void HandleChannelingInterruptTurn(ChannelingLogic.InitiativeHolder initiativeHolder, bool isTurnBased)
	{
		if (isTurnBased)
		{
			MechanicEntity maybeUnit = initiativeHolder.MaybeUnit;
			if (maybeUnit != null)
			{
				InterruptCurrentTurnImmediate(new UnitInterruptTurnParams(maybeUnit, initiativeHolder, new InterruptionData()));
				Game.Instance.Controllers.EntityDestroyer.Destroy(initiativeHolder);
			}
		}
	}

	private void StartUnitTurnInternal(MechanicEntity entity, bool isTurnBased)
	{
		if (isTurnBased && entity is UnitEntity unit)
		{
			unit.SnapToGrid();
		}
		if (entity.IsDirectlyControllable)
		{
			Game.Instance.Player.UISettings.StopSpeedUp();
		}
		if (entity.Initiative.InterruptingOrder < 1 && (entity.Initiative.WasPreparedForRound < CombatRound || entity.Initiative.PreparationInterrupted))
		{
			PrepareUnitForNewTurn(entity, isTurnBased);
			entity.GetAbilityCooldownsOptional()?.RemoveHandAbilityGroupsCooldown();
		}
		else if (entity.Initiative.InterruptingOrder > 0 && !entity.Initiative.PreparationInterrupted)
		{
			entity.GetAbilityCooldownsOptional()?.SaveCooldownData();
			entity.GetAbilityCooldownsOptional()?.ResetCooldowns();
		}
	}

	private void EndUnitTurn([CanBeNull] MechanicEntity unit, bool isTurnBased, bool isEndOfTurnRequested = false)
	{
		m_UnableToActUnitForceTurnEndTime = null;
		if (isTurnBased)
		{
			if (unit is UnitEntity unit2)
			{
				unit2.SnapToGrid();
			}
			foreach (MechanicEntity item in EntitiesInCombat)
			{
				if (!HandleEntityCommands(item, isEndOfTurnRequested))
				{
					return;
				}
			}
			PartReplaceUnitTransition partReplaceUnitTransition = unit?.GetReplaceUnitTransitionOptional();
			if (partReplaceUnitTransition != null && partReplaceUnitTransition.IsFromOwner && partReplaceUnitTransition.ToUnit != null && !partReplaceUnitTransition.IsFinished)
			{
				return;
			}
		}
		Data.EndTurnRequested = false;
		bool flag = unit != null && unit.Initiative.InterruptingOrder > 0;
		if (unit != null)
		{
			if (flag)
			{
				unit.GetAbilityCooldownsOptional()?.RestoreCooldownData();
				unit.Initiative.InterruptingOrder = 0;
				EventBus.RaiseEvent((IMechanicEntity)unit, m_InterruptTurnEndNotifierAction, isCheckRuntime: true);
			}
			else
			{
				TickAbilityCooldowns(unit, interrupt: false);
				if (!unit.Features.DoesNotCountTurns)
				{
					unit.Initiative.LastTurn = GameRound;
				}
				TurnEndNotifier.isTurnBased = isTurnBased;
				EventBus.RaiseEvent((IMechanicEntity)unit, m_TurnEndNotifierAction, isCheckRuntime: true);
			}
		}
		if (isTurnBased)
		{
			using (ContextData<InterruptTurnEndMark>.RequestIf(flag && !IsChannelingLogicInterruptTurn))
			{
				NextTurnTB();
			}
		}
		static bool HandleEntityCommands(MechanicEntity e, bool endOfTurnRequested)
		{
			PartUnitCommands partUnitCommands = e?.GetCommandsOptional();
			if (partUnitCommands != null)
			{
				if (!partUnitCommands.Empty && !e.IsDead && !endOfTurnRequested)
				{
					return false;
				}
				return partUnitCommands.InterruptAllInterruptible();
			}
			return true;
		}
	}

	private void NextTurnTB()
	{
		bool nextRound;
		CombatTurnType turnType;
		MechanicEntity entity = TurnOrder.NextTurn(out nextRound, out turnType);
		if (turnType != CombatTurnType.Roaming)
		{
			if (nextRound)
			{
				NextRound(isFirst: false);
			}
			if (CurrentUnit != null)
			{
				StartUnitTurn(entity, isTurnBased: true);
			}
		}
	}

	private static void TickAbilityCooldowns([CanBeNull] MechanicEntity unit, bool interrupt)
	{
		unit?.GetAbilityCooldownsOptional()?.TickCooldowns(interrupt);
	}

	private void PrepareUnitForNewTurn(MechanicEntity entity, bool isTurnBased)
	{
		entity.Initiative.WasPreparedForRound = CombatRound;
		entity.GetCombatStateOptional()?.PrepareForNewTurn(isTurnBased);
		if (isTurnBased)
		{
			entity.GetCommandsOptional()?.InterruptAllInterruptible();
		}
	}

	private void HandleUnitStartCombat(BaseUnitEntity unit)
	{
		if (TurnBasedModeActive)
		{
			if (unit.IsDead)
			{
				Logger.Error("Trying to force dead unit {0} to combat", unit);
			}
			else
			{
				PrepareUnitForNewTurn(unit, TurnBasedModeActive);
				TickAbilityCooldowns(unit, interrupt: false);
				ScheduleInitiativeRoll(unit);
			}
		}
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		HandleUnitJoinCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		HandleUnitLeaveCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	private void AddUnitsToCombat()
	{
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		foreach (BaseUnitEntity item in EntitiesInCombat.OfType<BaseUnitEntity>())
		{
			if (item.Initiative.Empty)
			{
				HandleUnitStartCombat(item);
				list.Add(item);
				if (item.Commands.IsRunning())
				{
					item.Commands.InterruptAllInterruptible();
				}
			}
		}
		list.SnapToGrid();
		TryRollInitiative();
	}

	private void HandleUnitJoinCombat(BaseUnitEntity unit)
	{
		HandleUnitStartCombat(unit);
		unit.SnapToGrid();
		if (!IsPreparationTurn || !unit.IsPlayerEnemy)
		{
			return;
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!item.CombatState.StartedCombatNearEnemy && IsInDeploymentRestrictionZone(item, unit))
			{
				item.CombatState.StartedCombatNearEnemy = true;
			}
		}
	}

	private void HandleUnitLeaveCombat(BaseUnitEntity unit)
	{
		unit.Initiative.Clear();
		unit.Initiative.Overrides.RemoveAll((InitiativeOverride o) => o.EtudeEnforcement);
		RemoveUnit(unit);
	}

	private void HandleCurrentUnitUnableToAct()
	{
		UnitSquad unitSquad = CurrentUnit as UnitSquad;
		if (unitSquad != null && unitSquad.Units.HasItem(delegate(UnitReference i)
		{
			IAbstractUnitEntity entity = i.Entity;
			return entity != null && entity.CanAct && entity.IsInGame;
		}))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = CurrentUnit as BaseUnitEntity;
		if ((baseUnitEntity != null && baseUnitEntity.CanAct && baseUnitEntity.IsInGame) || (baseUnitEntity == null && unitSquad == null))
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		if (!m_UnableToActUnitForceTurnEndTime.HasValue)
		{
			CameraFollowTaskParams toCasterOnMissedTurn = ConfigRoot.Instance.CameraRoot.CameraFollowSettings.Get().ToCasterOnMissedTurn;
			m_UnableToActUnitForceTurnEndTime = gameTime + (toCasterOnMissedTurn.CameraObserveTime + toCasterOnMissedTurn.BlendSettings.BlendTime).Seconds();
			if (baseUnitEntity != null)
			{
				EventBus.RaiseEvent((IEntity)baseUnitEntity, (Action<IUnitMissedTurnHandler>)delegate(IUnitMissedTurnHandler h)
				{
					h.HandleOnMissedTurn();
				}, isCheckRuntime: true);
			}
		}
		else if (m_UnableToActUnitForceTurnEndTime <= gameTime)
		{
			RequestEndTurn();
		}
	}

	private void HandleCurrentUnitChannelingLogicInterruptTurn()
	{
		if (!IsChannelingLogicInterruptTurn || !(CurrentUnit is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		if (!baseUnitEntity.Commands.Empty)
		{
			m_ChannelingLogicInterruptTurnEndTime = null;
			return;
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		if (!m_ChannelingLogicInterruptTurnEndTime.HasValue)
		{
			m_ChannelingLogicInterruptTurnEndTime = gameTime + 1.Seconds();
		}
		else if (m_ChannelingLogicInterruptTurnEndTime <= gameTime)
		{
			RequestEndTurn();
		}
	}

	private void RemoveUnit(BaseUnitEntity unit)
	{
		UnitPathManager.Instance.RemovePath(unit);
		if (CurrentUnit == unit)
		{
			RequestEndTurn();
		}
		PartSquad squadOptional = unit.GetSquadOptional();
		if (squadOptional == null)
		{
			return;
		}
		UnitSquad squad = squadOptional.Squad;
		if (squad != null && squad.Units.AllItems((UnitReference i) => i.Entity?.ToBaseUnitEntity().Initiative.Empty ?? true))
		{
			squad.Initiative.Clear();
			if (CurrentUnit == squad)
			{
				RequestEndTurn();
			}
		}
	}

	public void ScheduleInterruptTurn(MechanicEntity entityToGetTurn, MechanicEntity interruptionSource, InterruptionData interruptionData)
	{
		ScheduleInterruptTurn(new UnitInterruptTurnParams(entityToGetTurn, interruptionSource, interruptionData));
	}

	public void ScheduleInterruptTurn(UnitInterruptTurnParams @params)
	{
		if (m_ScheduledInterrupt == null)
		{
			m_ScheduledInterrupt = @params;
		}
		else
		{
			m_DelayedInterrupts.Add(@params);
		}
	}

	private void TryInterruptTurn()
	{
		if (m_ScheduledInterrupt != null)
		{
			InterruptCurrentTurn(m_ScheduledInterrupt);
			m_ScheduledInterrupt = null;
		}
	}

	private void InterruptCurrentTurn(UnitInterruptTurnParams @params)
	{
		if (!IsInTurnBasedCombat())
		{
			return;
		}
		if (CurrentUnit is UnitSquad)
		{
			m_DelayedInterrupts.Add(@params);
			return;
		}
		if (@params.InterruptionData.WaitForCommandsToFinish)
		{
			PartUnitCommands commandsOptional = CurrentUnit.GetCommandsOptional();
			if (commandsOptional != null)
			{
				@params.InterruptionData.WaitForCommandsToFinish = false;
				commandsOptional.AddToQueue(@params);
				return;
			}
		}
		InterruptCurrentTurnImmediate(@params);
	}

	private void InterruptCurrentTurnImmediate(UnitInterruptTurnParams @params)
	{
		if (!IsInTurnBasedCombat())
		{
			return;
		}
		if (IsChannelingLogicInterruptTurn)
		{
			m_DelayedInterrupts.Add(@params);
			return;
		}
		MechanicEntity mechanicEntity = @params.EntityToGetTheTurn;
		using (ContextData<InterruptTurnData>.Request().Setup(mechanicEntity, @params.InterruptionSource))
		{
			EventBus.RaiseEvent((IMechanicEntity)mechanicEntity, (Action<IInterruptCurrentTurnHandler>)delegate(IInterruptCurrentTurnHandler h)
			{
				h.HandleOnInterruptCurrentTurn();
			}, isCheckRuntime: true);
			TurnOrder.InterruptCurrentUnit(mechanicEntity);
			StartUnitTurn(mechanicEntity, isTurnBased: true, @params.InterruptionData);
			mechanicEntity.Initiative.PreparationInterrupted = true;
			Data.EndTurnRequested = false;
		}
	}

	public static bool IsInTurnBasedCombat()
	{
		if (Game.Instance.Player != null && Game.Instance.Player.IsInCombat)
		{
			return Game.Instance.CurrentModeType != GameModeType.Cutscene;
		}
		return false;
	}

	[Cheat(Name = "end_turn")]
	public static void TryEndPlayerTurnStatic()
	{
		Game.Instance.Controllers.TurnController.TryEndPlayerTurn();
	}

	public void OnAreaBeginUnloading()
	{
		m_OnAreaBeginUnloading = true;
	}

	public void OnAreaDidLoad()
	{
		m_OnAreaBeginUnloading = false;
	}

	public void NextRoundForManualCombat()
	{
		if (!IsManualCombatTurn || !TurnBasedModeActive)
		{
			throw new InvalidOperationException("TurnController.NextRoundForManualCombat() is invalid outside of manual combat");
		}
		NextRoundRT();
	}

	public void BeginPreparationTurn(bool canDeploy, bool raiseEvent = true)
	{
		if (!TurnBasedModeActive || CombatRound > 1)
		{
			throw new InvalidOperationException("BeginPreparationTurn");
		}
		m_DeploymentDiagnosticsSessionId++;
		LogDeployment("BeginPreparationTurn.enter", $"canDeploy={canDeploy}, raiseEvent={raiseEvent}, combatRound={CombatRound}, " + $"turnType={MaybeTurnOrder?.CurrentTurnType}, alreadyPreparation={IsPreparationTurn}, " + $"currentUnit={CurrentUnit}, party={FormatDeploymentPartyState()}");
		m_StartBattleSignal = SignalService.Instance.RegisterNext();
		TurnOrder.BeginPreparationTurn();
		LogDeployment("BeginPreparationTurn.afterTurnOrder", $"turnType={MaybeTurnOrder?.CurrentTurnType}, currentUnit={CurrentUnit}");
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.GetCombatStateOptional()?.SetMovementPoints(ConfigRoot.Instance.CombatRoot.DistanceInPreparationTurn, null);
		}
		BaseUnitEntity[] array = Game.Instance.EntityPools.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy).ToArray();
		LogDeployment("BeginPreparationTurn.restrictionScan", $"enemies={array.Length}, party={FormatDeploymentPartyState()}");
		foreach (BaseUnitEntity item2 in Game.Instance.Player.Party)
		{
			BaseUnitEntity[] array2 = array;
			foreach (BaseUnitEntity enemy in array2)
			{
				if (item2.CombatState.StartedCombatNearEnemy = IsInDeploymentRestrictionZone(item2, enemy))
				{
					break;
				}
			}
		}
		AddPreparationTurnVisualEffect();
		if (raiseEvent)
		{
			EventBus.RaiseEvent(delegate(IPreparationTurnBeginHandler h)
			{
				h.HandleBeginPreparationTurn(canDeploy);
			});
		}
		LogDeployment("BeginPreparationTurn.exit", "party=" + FormatDeploymentPartyState());
	}

	public void RequestEndPreparationTurn()
	{
		bool flag = CanFinishDeploymentPhase();
		LogDeployment("RequestEndPreparationTurn", $"isPreparation={IsPreparationTurn}, canFinish={flag}, " + $"signalEmpty={m_StartBattleSignal.IsEmpty}, party={FormatDeploymentPartyState()}");
		SignalService.Instance.CheckReadyOrSend(ref m_StartBattleSignal);
	}

	public void AddPreparationTurnVisualEffect()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.AddFact((BlueprintBuff)ConfigRoot.Instance.FxRoot.PreparationTurnVisualBuff);
		}
	}

	public void RemovePreparationTurnVisualEffect()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.Facts.Remove((BlueprintBuff)ConfigRoot.Instance.FxRoot.PreparationTurnVisualBuff);
		}
	}

	public void ForceEndPreparationTurn()
	{
		LogDeployment("ForceEndPreparationTurn.enter", $"turnBased={TurnBasedModeActive}, isPreparation={IsPreparationTurn}, combatRound={CombatRound}, " + $"turnType={MaybeTurnOrder?.CurrentTurnType}, currentUnit={CurrentUnit}, " + "party=" + FormatDeploymentPartyState());
		if (!TurnBasedModeActive || !IsPreparationTurn)
		{
			Logger.Error("ForceEndPreparationTurn invalid operation: " + $"TurnBasedModeActive={TurnBasedModeActive} IsPreparationTurn={IsPreparationTurn}");
			Logger.Warning(string.Format("[{0}] session={1} ", "WH2-51465", m_DeploymentDiagnosticsSessionId) + "event=ForceEndPreparationTurn.anomaly invalidState=true");
			return;
		}
		RemovePreparationTurnVisualEffect();
		TurnOrder.EndPreparationTurn();
		LogDeployment("ForceEndPreparationTurn.afterTurnOrder", $"turnType={MaybeTurnOrder?.CurrentTurnType}, currentUnit={CurrentUnit}, " + "party=" + FormatDeploymentPartyState());
		EventBus.RaiseEvent(delegate(IPreparationTurnEndHandler h)
		{
			h.HandleEndPreparationTurn();
		});
		LogDeployment("ForceEndPreparationTurn.exit", "party=" + FormatDeploymentPartyState());
	}

	private void LogDeployment(string eventName, string message)
	{
		Logger.Log(string.Format("[{0}] session={1} ", "WH2-51465", m_DeploymentDiagnosticsSessionId) + "event=" + eventName + " " + message);
	}

	private static string FormatDeploymentPartyState()
	{
		return string.Join(" | ", Game.Instance.Player.Party.Select(FormatDeploymentUnitState));
	}

	private static string FormatDeploymentUnitState(BaseUnitEntity unit)
	{
		PartUnitCombatState combatStateOptional = unit.GetCombatStateOptional();
		string text = unit.Commands.Current?.GetType().Name ?? "<none>";
		return $"unit={unit}, position={unit.Position}, inCombat={unit.IsInCombat}, " + $"deadOrUnconscious={unit.IsDeadOrUnconscious}, " + $"startedCombatNearEnemy={combatStateOptional?.StartedCombatNearEnemy}, " + $"canDeployNearEnemies={unit.Features.CanDeployNearEnemies}, " + "command=" + text;
	}

	public void BeginManualCombat()
	{
		if (TurnBasedModeActive)
		{
			throw new InvalidOperationException("BeginManualCombat");
		}
		ActiveEncounter.Start(ConfigRoot.Instance.EncounterRoot.DefaultEncounter);
		EnterTb();
		TurnOrder.BeginManualCombat();
	}

	public void EndManualCombat()
	{
		TurnOrder.EndManualCombat();
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		TryRestrictAbilityTillNextRound(context);
	}

	private static void TryRestrictAbilityTillNextRound(AbilityExecutionContext context)
	{
	}

	public bool GetStartBattleProgress(out int current, out int target, out NetPlayerGroup playerGroup)
	{
		return SignalService.Instance.GetProgress(m_StartBattleSignal, out current, out target, out playerGroup);
	}

	private void ApplyPostLoadFixes()
	{
		try
		{
			if (!IsSpaceCombat)
			{
				foreach (BaseUnitEntity unit in Game.Instance.EntityPools.AllBaseUnits.All)
				{
					if (unit.IsInCombat && unit.Suppressed && Game.Instance.Player.Group.Memory.Contains(unit))
					{
						BaseUnitEntity baseUnitEntity = Game.Instance.Player.Party.OrderBy((BaseUnitEntity i) => i.DistanceToInCells(unit)).FirstOrDefault();
						unit.Position = baseUnitEntity?.Position ?? Game.Instance.Player.MainCharacter.Entity.Position;
						unit.SnapToGrid();
					}
				}
			}
			if (!IsInTurnBasedCombat() || IsSpaceCombat)
			{
				return;
			}
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit.IsInCombat && !allBaseAwakeUnit.IsDead)
				{
					allBaseAwakeUnit.SnapToGrid();
				}
			}
		}
		catch (Exception exception)
		{
			Logger.ExceptionWithReport(exception, null);
		}
	}

	public void HandleCameraFocusEvent(Transform target, CameraFollowTaskParams taskParams, bool pauseCombatTurnOrder)
	{
		if (pauseCombatTurnOrder)
		{
			TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
			TimeSpan timeSpan = (taskParams.BlendSettings.BlendTime + taskParams.CameraObserveTime).Seconds();
			m_CustomEventEndTime = gameTime + timeSpan;
		}
	}
}
