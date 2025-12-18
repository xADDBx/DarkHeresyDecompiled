using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

public readonly struct ElevatorPlatformTransform
{
	public readonly Vector3 Position;

	public readonly float Orientation;

	public ElevatorPlatformTransform(Vector3 position, float orientation)
	{
		Position = position;
		Orientation = orientation;
	}
}
