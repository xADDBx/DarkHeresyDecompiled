using UnityEngine;

namespace Kingmaker.View;

public struct MovementStrategyInput
{
	public readonly bool WasTraverseInProgress;

	public readonly Vector2 FaceDirection;

	public readonly Vector2 MoveDirection;

	public MovementStrategyInput(bool wasTraverseInProgress, Vector2 faceDirection, Vector2 moveDirection)
	{
		WasTraverseInProgress = wasTraverseInProgress;
		FaceDirection = faceDirection;
		MoveDirection = moveDirection;
	}
}
