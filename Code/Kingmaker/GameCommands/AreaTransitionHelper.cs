using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.GameCommands;

public static class AreaTransitionHelper
{
	private const float ApproachRadiusToTransitionPoint = 1.5f;

	public static void StartAreaTransition([NotNull] MapObjectEntity mapObjectEntity)
	{
		if (Game.Instance.Player.IsInCombat || Game.Instance.CurrentModeType == GameModeType.Dialog)
		{
			return;
		}
		AreaTransitionPart areaTransition = mapObjectEntity.GetOptional<AreaTransitionPart>();
		if (areaTransition != null)
		{
			Vector3 position = mapObjectEntity.Position;
			Game.Instance.GameCommandQueue.ClearAreaTransitionGroupDuplicates();
			SelectionManagerFacade.SelectionManager.SelectAll();
			List<BaseUnitEntity> list = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList();
			List<EntityRef<BaseUnitEntity>> unitRefs = ((IEnumerable<BaseUnitEntity>)list).Select((Func<BaseUnitEntity, EntityRef<BaseUnitEntity>>)((BaseUnitEntity u) => u)).ToList();
			Guid groupGuid = Guid.NewGuid();
			UnitCommandsRunner.MoveSelectedUnitsToPointRT(ObstacleAnalyzer.GetDeepNavmeshPoint(position), ClickGroundHandler.GetDirection(position, list), Game.Instance.IsControllerGamepad, preview: false, ConfigRoot.Instance.Formations.MinSpaceFactor, list, delegate(BaseUnitEntity unit, MoveCommandSettings s)
			{
				RunUnitTransitionCommand(groupGuid, unitRefs, unit, areaTransition, s.Destination);
			});
		}
	}

	private static void RunUnitTransitionCommand(Guid groupGuid, List<EntityRef<BaseUnitEntity>> units, BaseUnitEntity unit, AreaTransitionPart transition, Vector3 position)
	{
		float approachRadiusMeters = Math.Max(0f, 1.5f - (transition.Owner.Position - position).magnitude);
		PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, position, approachRadiusMeters, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (unit.IsMovementLockedByGameModeOrCombat())
			{
				PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
			}
			else
			{
				UnitAreaTransitionParams cmdParams = new UnitAreaTransitionParams(groupGuid, units, position, transition)
				{
					IsSynchronized = true
				};
				if (path.vectorPath.Count > 0)
				{
					UnitMoveToParams cmdParams2 = new UnitMoveToParams(path, position, 0f)
					{
						IsSynchronized = true
					};
					unit.Commands.Run(cmdParams2);
					unit.Commands.AddToQueue(cmdParams);
				}
				else
				{
					unit.Commands.Run(cmdParams);
				}
				if (Game.Instance.IsPaused)
				{
					UnitCommandsRunner.ShowDestination(unit, position);
				}
				EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
				{
					h.HandleAreaTransition();
				});
			}
		});
	}
}
