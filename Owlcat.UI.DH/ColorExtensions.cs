using UnityEngine;

public static class ColorExtensions
{
	public static string HTML(this Color color)
	{
		return ColorUtility.ToHtmlStringRGB(color);
	}

	public static string HTML(this Color32 color)
	{
		return ColorUtility.ToHtmlStringRGB((Color)color);
	}
}
