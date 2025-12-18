using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityMath
{
	public static float ToFraction(int percent)
	{
		return (float)percent / 100f;
	}

	public static float ToFraction(float percent)
	{
		return percent / 100f;
	}

	public static int ToPercent(float fraction)
	{
		return Mathf.RoundToInt(fraction * 100f);
	}

	public static float Invert(float percent)
	{
		return 1f - percent;
	}

	public static float Invert(int percent)
	{
		return 100f - (float)percent;
	}
}
