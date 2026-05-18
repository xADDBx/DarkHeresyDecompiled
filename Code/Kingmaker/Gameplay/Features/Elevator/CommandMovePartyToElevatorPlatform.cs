using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("4be8a153c3a841b9b7ab29807fdfd20d")]
public sealed class CommandMovePartyToElevatorPlatform : CommandBase
{
	private sealed class CommandData
	{
		public UnitData[] Units = Array.Empty<UnitData>();

		public CommandResult? PathCalculationResult;

		public bool IsFinished;
	}

	private sealed class UnitData
	{
		public EntityRef<BaseUnitEntity> UnitRef;

		public UnitCommandHandle? Command;

		public Vector3 TargetPosition;

		public float TargetOrientation;

		public bool WaitingForPath;

		public BaseUnitEntity? Unit => UnitRef.Entity;
	}

	[SerializeField]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float Speed = 5f;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		ElevatorPlatformEntity elevatorPlatform = player.GetElevatorPlatform();
		elevatorPlatform.CutsceneHold.Retain();
		LocatorEntity[] array = elevatorPlatform.Config.PartyPlaces.Dereference().NotNull().ToArray();
		if (array.Length == 0)
		{
			return CommandResult.Fail($"No party places defined for elevator {elevatorPlatform}");
		}
		List<BaseUnitEntity> partyAndPets = Game.Instance.Player.PartyAndPets;
		CommandData commandData = player.GetCommandData<CommandData>(this);
		commandData.Units = new UnitData[partyAndPets.Count];
		for (int i = 0; i < partyAndPets.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = partyAndPets[i];
			UnitData unitData = (commandData.Units[i] = new UnitData());
			LocatorEntity locatorEntity = ((i >= array.Length) ? array[^1] : array[i]);
			unitData.UnitRef = baseUnitEntity;
			unitData.WaitingForPath = true;
			Vector3 targetPosition = (unitData.TargetPosition = locatorEntity.Position);
			float targetOrientation = (unitData.TargetOrientation = locatorEntity.Orientation);
			PathfindingService.Instance.FindPathRT_Delayed(baseUnitEntity.MovementAgent, targetPosition, 0.3f, 1, delegate(ForcedPath path)
			{
				unitData.WaitingForPath = false;
				if (path.error)
				{
					commandData.PathCalculationResult = CommandResult.Fail("An error path was returned. Ignoring. Unit [" + unitData.Unit?.UniqueId + ", '" + unitData.Unit?.Blueprint?.Name + "']");
				}
				else
				{
					BaseUnitEntity unit = unitData.Unit;
					if (unit == null || unit.IsDisposed)
					{
						path.Claim(this);
						path.Release(this);
						commandData.PathCalculationResult = CommandResult.Fail("Unit was disposed while searching for path");
					}
					else
					{
						UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, targetPosition)
						{
							MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation),
							Orientation = targetOrientation,
							OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null)
						};
						unitMoveToParams.MarkFromCutscene();
						unitData.Command = unitData.Unit.Commands.Run(unitMoveToParams);
					}
				}
			});
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		CommandData commandData = player.GetCommandData<CommandData>(this);
		CommandResult? pathCalculationResult = commandData.PathCalculationResult;
		if (pathCalculationResult.HasValue && !pathCalculationResult.GetValueOrDefault().IsSuccess)
		{
			return commandData.PathCalculationResult.Value;
		}
		if (time > (double)m_Timeout)
		{
			return Interrupt(player);
		}
		if (IsMovementFinished(player))
		{
			commandData.IsFinished = true;
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		CommandData commandData = player.GetCommandData<CommandData>(this);
		UnitData[] units = commandData.Units;
		foreach (UnitData unitData in units)
		{
			UnitCommandHandle command = unitData.Command;
			if (command != null && !command.IsFinished)
			{
				command.Interrupt();
			}
			BaseUnitEntity unit = unitData.Unit;
			if (unit != null && !unit.IsDisposed)
			{
				unit.Translocate(unitData.TargetPosition, unitData.TargetOrientation);
			}
		}
		commandData.Units = Array.Empty<UnitData>();
		player.GetElevatorPlatform().CutsceneHold.Release();
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		player.GetCommandData<CommandData>(this).IsFinished = true;
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		player.GetCommandData<CommandData>(this).IsFinished = true;
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<CommandData>(this).IsFinished;
	}

	private bool IsMovementFinished(CutscenePlayerData player)
	{
		UnitData[] units = player.GetCommandData<CommandData>(this).Units;
		foreach (UnitData unitData in units)
		{
			if (unitData == null)
			{
				goto IL_0037;
			}
			if (!unitData.WaitingForPath)
			{
				UnitCommandHandle command = unitData.Command;
				if (command == null || command.IsFinished)
				{
					goto IL_0037;
				}
			}
			bool flag = true;
			goto IL_003a;
			IL_0037:
			flag = false;
			goto IL_003a;
			IL_003a:
			if (flag)
			{
				return false;
			}
		}
		return true;
	}
}
