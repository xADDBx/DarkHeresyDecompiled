using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("c76bfe7613c12d54d8f8c3e28cc6922d")]
public sealed class CommandStartElevatorTransition : CommandBase
{
	[AllowedEntityType(typeof(ElevatorPlatformView))]
	public EntityReference Elevator = new EntityReference();

	[AllowedEntityType(typeof(ElevatorPlatformStopView))]
	public EntityReference Destination = new EntityReference();

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		ElevatorPlatformEntity elevatorPlatformEntity = (ElevatorPlatformEntity)(Elevator.FindData() ?? throw new InvalidOperationException($"Elevator entity not found: {Elevator}"));
		ElevatorPlatformStopEntity destination = (ElevatorPlatformStopEntity)(Destination.FindData() ?? throw new InvalidOperationException($"Destination stop not found: {Destination}"));
		elevatorPlatformEntity.StartTransitionWithoutCutscene(destination);
		player.Parameters.Params["Elevator"] = new NamedParameterValue_MapObject(elevatorPlatformEntity);
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		(Elevator.FindData() as ElevatorPlatformEntity)?.ForceComplete();
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (Elevator.FindData() is ElevatorPlatformEntity elevatorPlatformEntity)
		{
			return elevatorPlatformEntity.IsIdle;
		}
		return true;
	}
}
