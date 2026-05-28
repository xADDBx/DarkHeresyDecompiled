using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal class FreeCameraMode : InputControllerBase.Mode
{
	public override void OnUpdate(InputContext context, InputFrame frame)
	{
		Vector2 cameraMovement = InputUtility.SmoothStepMagnitude(frame.RightStick + frame.CameraMove);
		context.SetCameraMovement(cameraMovement);
		float x = InputUtility.SmootherStepAxis(frame.CameraOrbit.x);
		float y = InputUtility.SmootherStepAxis(frame.CameraOrbit.y);
		context.SetCameraOrbit(new Vector2(x, y));
	}
}
