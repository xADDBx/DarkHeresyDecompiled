using System.Linq;
using UnityEngine;

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
			for (int i = 0; i < Transitions.Length; i++)
			{
				ElevatorPlatformTransitionSettings elevatorPlatformTransitionSettings = Transitions[i];
				Transitions[i] = new ElevatorPlatformTransitionSettings
				{
					MovementSpeed = elevatorPlatformTransitionSettings.MovementSpeed,
					MovementSpeedCurve = ReverseCurve(elevatorPlatformTransitionSettings.MovementSpeedCurve),
					Rotation = elevatorPlatformTransitionSettings.Rotation,
					RotationCurve = new AnimationCurve(elevatorPlatformTransitionSettings.RotationCurve.keys),
					RotationDuration = elevatorPlatformTransitionSettings.RotationDuration
				};
			}
		}
	}

	private static AnimationCurve ReverseCurve(AnimationCurve source)
	{
		Keyframe[] keys = source.keys;
		Keyframe[] array = new Keyframe[keys.Length];
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[keys.Length - 1 - i];
			array[i] = new Keyframe(1f - keyframe.time, keyframe.value, 0f - keyframe.outTangent, 0f - keyframe.inTangent);
		}
		return new AnimationCurve(array);
	}
}
