using Kingmaker.GameModes;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityRect
{
	public static Vector2 ObjectPixelPositionInRect(Vector3 objectPosition, Transform parent)
	{
		Vector3 objectPositionInCamera = GetObjectPositionInCamera(objectPosition);
		Rect rect = ((RectTransform)parent.parent).rect;
		Vector2 result = default(Vector2);
		result.x = objectPositionInCamera.x * rect.width;
		result.y = objectPositionInCamera.y * rect.height;
		return result;
	}

	private static Vector3 GetObjectPositionInCamera(Vector3 objectPosition)
	{
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			return CameraRig.WorldToViewportMainCamera(objectPosition);
		}
		return CameraRig.Instance.WorldToViewport(objectPosition);
	}
}
