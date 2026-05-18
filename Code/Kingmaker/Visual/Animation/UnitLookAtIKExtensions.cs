using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public static class UnitLookAtIKExtensions
{
	public static void LookAt(this IAbstractUnitEntity unit, Vector3 position, RotatedBonesSet rotatedBonesSet = RotatedBonesSet.HeadAndSpine, float turningTime = 0.3f, float duration = 0f)
	{
		LookAtIKController componentInChildren = unit.View.GO.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren == null)
		{
			unit.TurnTo(position);
			return;
		}
		if (unit.GetDeltaAngle(position) > 80f)
		{
			componentInChildren.PushResetCommand();
			unit.TurnTo(position);
			return;
		}
		componentInChildren.PushLookAtCommand(new ConstantPositionProvider(position), rotatedBonesSet, turningTime, duration);
		if (duration > 0f)
		{
			componentInChildren.PushResetCommand(turningTime);
		}
	}

	public static void LookAtWithoutTurnTo(this IAbstractUnitEntity unit, Vector3 position, RotatedBonesSet rotatedBonesSet = RotatedBonesSet.HeadAndSpine, float turningTime = 0.3f, float duration = 0f)
	{
		unit.LookAtWithoutTurnTo(new ConstantPositionProvider(position), rotatedBonesSet, turningTime, duration);
	}

	public static void LookAtWithoutTurnTo(this IAbstractUnitEntity unit, IVector3PositionProvider positionProvider, RotatedBonesSet rotatedBonesSet = RotatedBonesSet.HeadAndSpine, float turningTime = 0.3f, float duration = 0f)
	{
		LookAtIKController componentInChildren = unit.View.GO.GetComponentInChildren<LookAtIKController>();
		if (!(componentInChildren == null))
		{
			componentInChildren.PushLookAtCommand(positionProvider, rotatedBonesSet, turningTime, duration);
			if (duration > 0f)
			{
				componentInChildren.PushResetCommand(turningTime);
			}
		}
	}

	public static bool IsLookingAt(this IAbstractUnitEntity unit, Vector3 targetPosition, float? targetAngleHint = null)
	{
		LookAtIKController componentInChildren = unit.View.GO.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren != null && componentInChildren.IsLookingAt(targetPosition))
		{
			return true;
		}
		float angle = targetAngleHint ?? unit.GetOrientationTo(targetPosition);
		return unit.GetDeltaAngle(angle) == 0f;
	}

	public static bool IsFinishedLookAtCommands(this IAbstractUnitEntity unit)
	{
		LookAtIKController componentInChildren = unit.View.GO.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren == null)
		{
			return true;
		}
		return !componentInChildren.HasCommandToProcess();
	}

	public static void StopLookAt(this IAbstractUnitEntity unit, float turningTime = 0.3f)
	{
		LookAtIKController componentInChildren = unit.View.GO.GetComponentInChildren<LookAtIKController>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetImmediately(turningTime);
		}
	}

	public static float GetDeltaAngle(float angle1, float angle2)
	{
		return Mathf.Abs(Mathf.DeltaAngle(angle1, angle2));
	}

	public static float GetDeltaAngle(this IAbstractUnitEntity unit, float angle)
	{
		return Mathf.Abs(Mathf.DeltaAngle(unit.Orientation, angle));
	}

	public static float GetDeltaAngle(this IAbstractUnitEntity unit, Vector3 position)
	{
		return Mathf.Abs(Mathf.DeltaAngle(unit.Orientation, unit.GetOrientationTo(position)));
	}
}
