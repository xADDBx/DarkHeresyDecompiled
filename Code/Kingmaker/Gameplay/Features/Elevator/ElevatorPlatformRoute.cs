using System.Linq;

namespace Kingmaker.Gameplay.Features.Elevator;

public sealed class ElevatorPlatformRoute
{
	public readonly ElevatorPlatformStopEntity From;

	public readonly ElevatorPlatformStopEntity To;

	public readonly ElevatorPlatformTransform[] Waypoints;

	public readonly ElevatorPlatformTransitionSettings[] Transitions;

	public ElevatorPlatformRoute(IElevatorPlatformRouteSettings settings, bool invert)
	{
		From = (invert ? settings.To : settings.From);
		To = (invert ? settings.From : settings.To);
		Waypoints = (invert ? settings.Waypoints.Reverse().ToArray() : settings.Waypoints.ToArray());
		Transitions = (invert ? settings.Transitions.Reverse().ToArray() : settings.Transitions.ToArray());
		if (invert)
		{
			ElevatorPlatformTransitionSettings[] transitions = Transitions;
			foreach (ElevatorPlatformTransitionSettings elevatorPlatformTransitionSettings in transitions)
			{
				elevatorPlatformTransitionSettings.MovementSpeedCurve.keys = elevatorPlatformTransitionSettings.MovementSpeedCurve.keys.Reverse().ToArray();
			}
		}
	}
}
