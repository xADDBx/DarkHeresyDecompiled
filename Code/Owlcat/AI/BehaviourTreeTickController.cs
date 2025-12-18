using System;
using Kingmaker;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Squads;

namespace Owlcat.AI;

public class BehaviourTreeTickController : IControllerTick, IController, IControllerReset, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler
{
	public readonly BehaviourTreeRuntimeStorage Storage = new BehaviourTreeRuntimeStorage();

	private TurnController TurnController => Game.Instance.Controllers.TurnController;

	private bool IsChannelingLogicInterruptTurn => TurnController.IsChannelingLogicInterruptTurn;

	private TimeSpan GameDeltaTimeSpan => Game.Instance.Controllers.TimeController.GameDeltaTimeSpan;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnReset()
	{
	}

	public void Tick()
	{
		if (!TurnController.IsAiTurn || IsChannelingLogicInterruptTurn)
		{
			return;
		}
		MechanicEntity currentUnit = TurnController.CurrentUnit;
		if (currentUnit == null || HasActiveChannelingLogic(currentUnit) || TurnController.EndTurnRequested)
		{
			return;
		}
		bool endTurn;
		if (currentUnit is UnitSquad unitSquad)
		{
			int num = 0;
			foreach (UnitReference unit in unitSquad.Units)
			{
				TickSquadUnit(unit.ToBaseUnitEntity(), out endTurn);
				if (endTurn)
				{
					num++;
				}
			}
			endTurn = num >= unitSquad.Count;
		}
		else
		{
			TickUnit(currentUnit, out endTurn);
		}
		if (endTurn)
		{
			currentUnit.GetCommandsOptional()?.InterruptAll((AbstractUnitCommand cmd) => true);
			EndUnitTurn(currentUnit);
		}
	}

	private void TickSquadUnit(MechanicEntity unit, out bool endTurn)
	{
		if (Storage.TryGetRuntime(unit, out var runtime) && !(runtime.RuntimeBridge.BehaviourTree.Root.Child is SquadControlNode))
		{
			PFLog.AI.Log("SquadControlNode was not found, skip Squad turn");
			endTurn = true;
		}
		else
		{
			TickUnit(unit, out endTurn);
		}
	}

	private void TickUnit(MechanicEntity unit, out bool endTurn)
	{
		MechanicEntityBehaviourTreeRuntime runtime;
		if (!unit.CanAct || !unit.IsInGame)
		{
			endTurn = true;
		}
		else if (!Storage.TryGetRuntime(unit, out runtime))
		{
			PFLog.AI.Error("Brain is null, end turn");
			endTurn = true;
		}
		else
		{
			runtime.Tick(GameDeltaTimeSpan, out endTurn);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && TurnController.IsAiTurn)
		{
			PrepareEntityBeforeStartTurn(EventInvokerExtensions.MechanicEntity);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (TurnController.IsAiTurn)
		{
			PrepareEntityBeforeStartTurn(EventInvokerExtensions.MechanicEntity);
		}
	}

	private void PrepareEntityBeforeStartTurn(MechanicEntity unit)
	{
		if (unit is UnitSquad unitSquad)
		{
			InitSquadData(unitSquad);
			foreach (UnitReference unit2 in unitSquad.Units)
			{
				PrepareRuntime(unit2.ToBaseUnitEntity());
			}
		}
		else
		{
			PrepareRuntime(unit);
		}
		PFLog.AI.Log($"[ START TURN ]: {unit}");
	}

	private void PrepareRuntime(MechanicEntity unit)
	{
		if (Storage.TryGetRuntime(unit, out var runtime))
		{
			runtime.StartPreparationTask();
		}
	}

	private void EndUnitTurn(MechanicEntity unit)
	{
		TurnController.RequestEndTurn();
		if (Storage.TryGetRuntime(unit, out var runtime))
		{
			runtime.StoreRuntimeState();
		}
		PFLog.AI.Log($"[ END TURN ]: {unit}");
	}

	private static void InitSquadData(UnitSquad squad)
	{
		squad.Data.Reset();
	}

	private static bool HasActiveChannelingLogic(MechanicEntity entity)
	{
		return entity.GetOptional<PartChanneling>() != null;
	}
}
