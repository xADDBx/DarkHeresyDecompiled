using UnityEngine;

namespace Kingmaker.View;

public struct MovementStrategyProcessingResult
{
	public enum MovementState
	{
		Completed,
		InProgress,
		Interrupted
	}

	public bool IsPositionChanged;

	public bool IsTraverseInProgress;

	public float EstimatedTimeLeft;

	public Vector2 FaceDirection;

	public Vector2 MoveDirection;

	public MovementState State;
}
