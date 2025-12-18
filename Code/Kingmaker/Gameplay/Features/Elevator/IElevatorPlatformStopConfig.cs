using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

public interface IElevatorPlatformStopConfig
{
	string EntityId { get; }

	bool IsInGame { get; }

	Vector3 Position { get; }

	float Orientation { get; }
}
