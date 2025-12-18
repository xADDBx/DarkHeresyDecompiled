using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Gameplay.Features.Elevator;

public interface IElevatorPlatformRouteSettings
{
	EntityRef<ElevatorPlatformStopEntity> From { get; }

	EntityRef<ElevatorPlatformStopEntity> To { get; }

	IEnumerable<ElevatorPlatformTransform> Waypoints { get; }

	IEnumerable<ElevatorPlatformTransitionSettings> Transitions { get; }
}
