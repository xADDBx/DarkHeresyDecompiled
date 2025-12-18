using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("066fa891d2a54ff8901b27a1e9fc57c5")]
public sealed class CheckElevatorStop : Condition
{
	[AllowedEntityType(typeof(ElevatorPlatformView))]
	public EntityReference Elevator = new EntityReference();

	[AllowedEntityType(typeof(ElevatorPlatformStopView))]
	public EntityReference Stop = new EntityReference();

	protected override string GetConditionCaption()
	{
		return $"Elevator {Elevator} is idle and on stop {Stop}";
	}

	protected override bool CheckCondition()
	{
		ElevatorPlatformEntity elevatorPlatformEntity = (ElevatorPlatformEntity)(Elevator.FindData() ?? throw new NullReferenceException());
		ElevatorPlatformStopEntity elevatorPlatformStopEntity = (ElevatorPlatformStopEntity)(Stop.FindData() ?? throw new NullReferenceException());
		if (elevatorPlatformEntity.IsIdle)
		{
			return elevatorPlatformEntity.CurrentStop == elevatorPlatformStopEntity;
		}
		return false;
	}
}
