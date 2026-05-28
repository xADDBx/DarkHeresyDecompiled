using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

internal static class InputUtility
{
	private const float kDeadZone = 0.05f;

	public static Vector2 GetDirectionTowardCamera(Vector2 screenSpaceDirection, Camera camera)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
		Vector3 right = camera.transform.right;
		Vector3 vector = normalized * screenSpaceDirection.y + right * screenSpaceDirection.x;
		return new Vector2(vector.x, vector.z);
	}

	public static float SmoothStep01(float value)
	{
		return Mathf.SmoothStep(0f, 1f, value);
	}

	public static float SmootherStep01(float value)
	{
		float num = Mathf.Clamp01(value);
		return num * num * num * (num * (num * 6f - 15f) + 10f);
	}

	public static Vector2 SmoothStepMagnitude(Vector2 value, float dz = 0.05f)
	{
		float num = Mathf.InverseLerp(dz, 1f, value.magnitude);
		if (Mathf.Approximately(num, 0f))
		{
			return Vector2.zero;
		}
		value *= SmoothStep01(num) / num;
		return value;
	}

	public static float SmootherStepAxis(float value, float dz = 0.05f)
	{
		float value2 = Mathf.InverseLerp(dz, 1f, Mathf.Abs(value));
		return Mathf.Sign(value) * SmootherStep01(value2);
	}
}
