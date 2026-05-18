using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitMovableAreaController : IControllerDisable, IController, IControllerTick, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitCommandActHandler, IUnitActionPointsHandler, IUnitSpentMovementPoints, IUnitGainMovementPoints, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetRoleSetHandler, IDirectMovementHandler, IUnitGetAbilityJump, ISubscriber<IBaseUnitEntity>, IUnitCombatHandler, IAreaActivationHandler
{
	private bool m_Dirty;

	private bool m_Hidden;

	private IDisposable m_CurrentUnitSubscription;

	private HashSet<GraphNode> m_ForbiddenToMoveAndCast = new HashSet<GraphNode>();

	private Dictionary<string, Vector3> m_InitialPositions = new Dictionary<string, Vector3>();

	public BaseUnitEntity CurrentUnit { get; private set; }

	public bool DeploymentPhase { get; private set; }

	public List<GraphNode> CurrentUnitMovableArea { get; private set; }

	public List<GraphNode> DeploymentForbiddenArea { get; private set; }

	public List<GraphNode> ThreateningArea { get; private set; }

	TickType IControllerTick.GetTickType()
	{
		return TickType.EndOfFrame;
	}

	void IControllerDisable.OnDisable()
	{
		Clear();
	}

	void IControllerTick.Tick()
	{
		if (m_Dirty && !m_Hidden)
		{
			UpdateMovableArea();
		}
		m_Dirty = false;
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (ShouldHandle(command.Executor) && (command is UnitMoveToProper || command is UnitUseAbility))
		{
			HideMovableArea();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility))
		{
			UpdateMovableAreaIfNeeded(command.Executor);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateMovableAreaIfNeeded(command.Executor);
	}

	public void HandleRestoreActionPoints()
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleDirectMovementStarted(ForcedPath path, bool disableAttacksOfOpportunity)
	{
	}

	public void HandleDirectMovementEnded()
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleActionPointsSpent(BaseUnitEntity unit)
	{
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleUnitGainMovementPoints(float movementPoints, IEvalContext context)
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	private void UpdateMovableAreaIfNeeded(AbstractUnitEntity unit)
	{
		if (ShouldHandle(unit))
		{
			UpdateMovableArea();
		}
	}

	private bool ShouldHandle(AbstractUnitEntity unitEntity)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			return unitEntity == CurrentUnit;
		}
		return false;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	private void HandleNewUnitStartTurn(MechanicEntity entity)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			Clear();
			UpdateMovableAreaForParty();
			MechanicEntity mechanicEntity = (Game.Instance.Controllers.TurnController.IsPreparationTurn ? (entity ?? Game.Instance.Controllers.TurnController.CurrentUnit) : (Game.Instance.Controllers.TurnController.CurrentUnit ?? entity));
			if (mechanicEntity is BaseUnitEntity currentUnit && mechanicEntity.IsDirectlyControllable)
			{
				CurrentUnit = currentUnit;
				UpdateMovableArea();
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Clear();
			ClearInitialPositions();
		}
	}

	private void UpdateMovableAreaForParty()
	{
		foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
		{
			IAbstractUnitEntity entity = partyCharacter.Entity;
			if (entity != null && !m_InitialPositions.ContainsKey(entity.UniqueId))
			{
				m_InitialPositions.Add(entity.UniqueId, entity.Position);
			}
		}
	}

	private void UpdateMovableArea()
	{
		if (CurrentUnit == null)
		{
			return;
		}
		Vector3 vector;
		if (DeploymentPhase)
		{
			if (m_InitialPositions.TryGetValue(CurrentUnit.UniqueId, out var value))
			{
				vector = value;
			}
			else
			{
				vector = CurrentUnit.Position;
				m_InitialPositions[CurrentUnit.UniqueId] = vector;
			}
		}
		else
		{
			vector = CurrentUnit.Position;
		}
		Dictionary<GraphNode, WarhammerPathPlayerCell> movableArea = GetMovableArea(CurrentUnit, vector);
		if (movableArea == null)
		{
			CurrentUnitMovableArea = null;
			ThreateningArea = null;
			DeploymentForbiddenArea = null;
		}
		else
		{
			CurrentUnitMovableArea = movableArea.Keys.ToList();
			ThreateningArea = (from p in movableArea
				where p.Value.IsOneWayPath
				select p.Key).ToList();
			DeploymentForbiddenArea = ((DeploymentPhase && movableArea != null) ? GetDeploymentForbiddenArea(CurrentUnit, ThreateningArea).ToList() : null);
		}
		m_Hidden = false;
		EventBus.RaiseEvent((IMechanicEntity)CurrentUnit, (Action<IUnitMovableAreaHandler>)delegate(IUnitMovableAreaHandler h)
		{
			h.HandleSetUnitMovableArea(CurrentUnitMovableArea);
		}, isCheckRuntime: true);
	}

	private Dictionary<GraphNode, WarhammerPathPlayerCell> GetMovableArea(BaseUnitEntity unit, Vector3 position)
	{
		float movementPoints = unit.CombatState.MovementPoints;
		if (!(movementPoints > 0f))
		{
			return null;
		}
		return PathfindingService.Instance.FindAllReachableTiles_Blocking(unit.View.MovementAgent, position, movementPoints, ignoreThreateningAreaCost: false, DeploymentPhase);
	}

	private static IEnumerable<GraphNode> GetDeploymentForbiddenArea(BaseUnitEntity unit, List<GraphNode> threateningArea)
	{
		HashSet<GraphNode> hashSet = TempHashSet.Get<GraphNode>();
		hashSet.AddRange(threateningArea);
		IEnumerable<ScriptZoneEntity> source = Game.Instance.EntityPools.ScriptZones.Where((ScriptZoneEntity area) => area.IsActive && area.Facts.HasComponent<ScriptZoneNoDeployment>());
		if ((bool)unit.Features.CanDeployNearEnemies)
		{
			return source.SelectMany((ScriptZoneEntity area) => area.Config.Shapes.SelectMany(EditorGridHelper.GetNodesInsideScriptZone));
		}
		hashSet.AddRange(source.SelectMany((ScriptZoneEntity area) => area.Config.Shapes.SelectMany(EditorGridHelper.GetNodesInsideScriptZone)));
		foreach (BaseUnitEntity item in Game.Instance.EntityPools.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy))
		{
			GridNode currentUnwalkableNode = item.CurrentUnwalkableNode;
			foreach (GridNodeBase item2 in GridAreaHelper.GetNodesSpiralAround(currentUnwalkableNode, item.SizeRect, 1))
			{
				if (item.DistanceToInCells(item2.Vector3Position()) <= 1)
				{
					Linecast.HasConnectionTransition condition = Linecast.HasConnectionTransition.Instance;
					if (!Linecast.LinecastGrid(currentUnwalkableNode.Graph, item.Position, item2.Vector3Position(), currentUnwalkableNode, out var _, ref condition))
					{
						hashSet.Add(item2);
					}
				}
			}
		}
		return hashSet.ToArray();
	}

	private void HideMovableArea()
	{
		m_Hidden = true;
		EventBus.RaiseEvent(delegate(IUnitMovableAreaHandler h)
		{
			h.HandleRemoveUnitMovableArea();
		});
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(turnController.IsDeploymentAllowed);
			return;
		}
		Clear();
		MechanicEntity currentUnit = turnController.CurrentUnit;
		if (turnController.TurnBasedModeActive && currentUnit is BaseUnitEntity currentUnit2 && currentUnit.IsDirectlyControllable)
		{
			CurrentUnit = currentUnit2;
			UpdateMovableArea();
		}
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		DeploymentPhase = canDeploy;
		if (canDeploy)
		{
			ClearInitialPositions();
			m_CurrentUnitSubscription?.Dispose();
			m_CurrentUnitSubscription = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Subscribe(HandleNewUnitStartTurn);
		}
	}

	public void HandleEndPreparationTurn()
	{
		m_CurrentUnitSubscription?.Dispose();
		DeploymentPhase = false;
		ClearInitialPositions();
	}

	public void ClearInitialPositions()
	{
		m_InitialPositions.Clear();
	}

	public void Clear()
	{
		HideMovableArea();
		UnitPathManager.Instance.RemoveAllPaths();
		UnitCommandsRunner.CancelMoveCommand();
		CurrentUnit = null;
	}

	public bool TryGetInitialPosition(BaseUnitEntity unit, out Vector3 initialPosition)
	{
		initialPosition = Vector3.zero;
		if (m_InitialPositions == null || unit == null)
		{
			return false;
		}
		return m_InitialPositions.TryGetValue(unit.UniqueId, out initialPosition);
	}

	public void ApplyInitialPosition(BaseUnitEntity unit, Vector3 initialPosition)
	{
		if (m_InitialPositions != null && unit != null)
		{
			m_InitialPositions.TryAdd(unit.UniqueId, initialPosition);
			UpdateMovableAreaIfNeeded(unit);
		}
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		UnitCommandsRunner.HandleRoleSet(entityId);
	}

	public void HandleUnitAbilityJumpDidActed(int distanceInCells)
	{
		if (ShouldHandle(EventInvokerExtensions.BaseUnitEntity))
		{
			UpdateMovableArea();
		}
	}

	public void HandleUnitResultJump(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster, bool useAttack)
	{
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		m_Dirty = true;
		if (DeploymentPhase)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			if (baseUnitEntity != null && baseUnitEntity.IsInPlayerParty)
			{
				m_InitialPositions[baseUnitEntity.UniqueId] = baseUnitEntity.Position;
			}
		}
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		m_Dirty = true;
	}

	public void OnAreaActivated()
	{
		m_Dirty = true;
	}
}
