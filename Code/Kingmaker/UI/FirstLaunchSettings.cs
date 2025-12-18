using UnityEngine;

namespace Kingmaker.UI;

public static class FirstLaunchSettings
{
	private const string FIRST_LAUNCH_PREF_KEY = "first_open_first_launch_settings";

	public static bool HasShown => PlayerPrefs.GetInt("first_open_first_launch_settings", 0) == 1;

	public static void SetHasShownValue(int value)
	{
		PlayerPrefs.SetInt("first_open_first_launch_settings", 1);
		PlayerPrefs.Save();
	}
}
