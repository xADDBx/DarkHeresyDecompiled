using UnityEngine;

namespace Kingmaker.View;

public interface IUnitMovementStrategy
{
	float Speed { get; }

	Vector3 Velocity { get; }

	Vector3? Destination { get; }

	bool IsReallyMoving { get; }

	void TickMovement(float deltaTime, MovementStrategyInput input, out MovementStrategyProcessingResult result);

	void Stop();
}
