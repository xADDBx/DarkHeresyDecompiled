using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class FreeMovementMode : InputControllerBase.Mode
{
	public override void OnUpdate(InputContext context, InputFrame frame)
	{
		Vector2 unitMovement = InputUtility.SmoothStepMagnitude(InputUtility.GetDirectionTowardCamera(frame.LeftStick, context.CameraRig.Camera));
		context.SetUnitMovement(unitMovement);
		float x = InputUtility.SmootherStepAxis(frame.RightStick.x + frame.CameraOrbit.x);
		float y = InputUtility.SmootherStepAxis(frame.RightStick.y + frame.CameraOrbit.y);
		context.SetCameraOrbit(new Vector2(x, y));
	}

	public override void OnExit(InputContext context)
	{
		context.SetUnitMovement(Vector2.zero);
	}
}
