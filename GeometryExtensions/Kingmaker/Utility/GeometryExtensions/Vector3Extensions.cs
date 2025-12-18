using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Vector3Extensions
{
	public static Vector3 Round(this Vector3 value, int decimals)
	{
		return new Vector3((float)Math.Round(value.x, decimals), (float)Math.Round(value.y, decimals), (float)Math.Round(value.z, decimals));
	}

	public static bool Approximately(this Vector3 a, Vector3 b)
	{
		if (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y))
		{
			return Mathf.Approximately(a.z, b.z);
		}
		return false;
	}

	public static Vector3 ReplaceY(this Vector3 self, float y)
	{
		return new Vector3(self.x, y, self.z);
	}

	public static Vector3 NormalizedEulerAngles(this Vector3 eulerAngles)
	{
		eulerAngles.x = NormalizeAngle(eulerAngles.x);
		eulerAngles.y = NormalizeAngle(eulerAngles.y);
		eulerAngles.z = NormalizeAngle(eulerAngles.z);
		return eulerAngles;
	}

	public static Vector3 DenormalizedEulerAngles(this Vector3 normalizedEulerAngles)
	{
		normalizedEulerAngles.x = DenormalizeAngle(normalizedEulerAngles.x);
		normalizedEulerAngles.y = DenormalizeAngle(normalizedEulerAngles.y);
		normalizedEulerAngles.z = DenormalizeAngle(normalizedEulerAngles.z);
		return normalizedEulerAngles;
	}

	private static float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		else if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	private static float DenormalizeAngle(float angle)
	{
		if (angle < 0f)
		{
			angle += 360f;
		}
		return angle;
	}
}
