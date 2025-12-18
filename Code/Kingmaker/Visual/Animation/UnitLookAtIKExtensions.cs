using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public static class UnitLookAtIKExtensions
{
	public static void LookAt(this AbstractUnitEntity unit, Vector3 position, float turningTime = 0.3f, float duration = 0f)
	{
		LookAtIKController componentInChildren = unit.View.gameObject.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren == null)
		{
			unit.TurnTo(position);
		}
		else if (unit.GetDeltaAngle(position) > 80f)
		{
			componentInChildren.StopLookAt();
			unit.TurnTo(position);
		}
		else
		{
			componentInChildren.StartLookAt(position, turningTime, duration);
		}
	}

	public static void LookAtWithoutTurnTo(this AbstractUnitEntity unit, Vector3 position, float maxDeltaAngle = 80f, float turningTime = 0.3f, float duration = 0f)
	{
		LookAtIKController componentInChildren = unit.View.gameObject.GetComponentInChildren<LookAtIKController>();
		if (!(componentInChildren == null))
		{
			if (unit.GetDeltaAngle(position) > Mathf.Min(80f, maxDeltaAngle))
			{
				componentInChildren.StopLookAt();
			}
			else
			{
				componentInChildren.StartLookAt(position, turningTime, duration);
			}
		}
	}

	public static bool IsLookingAt(this AbstractUnitEntity unit, Vector3 targetPosition, float? targetAngleHint = null)
	{
		LookAtIKController componentInChildren = unit.View.gameObject.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren != null && componentInChildren.IsLookingAt(targetPosition))
		{
			return true;
		}
		float angle = targetAngleHint ?? unit.GetOrientationTo(targetPosition);
		return unit.GetDeltaAngle(angle) == 0f;
	}

	public static void StopLookAt(this AbstractUnitEntity unit, float turningTime = 0.3f)
	{
		LookAtIKController componentInChildren = unit.View.gameObject.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren != null)
		{
			componentInChildren.StopLookAt(turningTime);
		}
	}

	public static float GetDeltaAngle(float angle1, float angle2)
	{
		return Mathf.Abs(Mathf.DeltaAngle(angle1, angle2));
	}

	public static float GetDeltaAngle(this AbstractUnitEntity unit, float angle)
	{
		return Mathf.Abs(Mathf.DeltaAngle(unit.Orientation, angle));
	}

	public static float GetDeltaAngle(this AbstractUnitEntity unit, Vector3 position)
	{
		return Mathf.Abs(Mathf.DeltaAngle(unit.Orientation, unit.GetOrientationTo(position)));
	}
}
