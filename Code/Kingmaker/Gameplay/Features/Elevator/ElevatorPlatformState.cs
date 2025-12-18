namespace Kingmaker.Gameplay.Features.Elevator;

public enum ElevatorPlatformState
{
	Idle,
	PrepareToTransition,
	StartTransitionToWaypoint,
	MoveToWaypoint,
	RotateOnWaypoint,
	EndTransitionToWaypoint
}
