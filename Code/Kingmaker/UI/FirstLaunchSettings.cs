using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.UI;

public static class FirstLaunchSettings
{
	private const string FIRST_LAUNCH_PREF_KEY = "first_open_first_launch_settings";

	public static bool HasShown
	{
		get
		{
			if (!SettingsController.Instance.GeneralSettingsProvider.HasKey("first_open_first_launch_settings"))
			{
				return PlayerPrefs.GetInt("first_open_first_launch_settings", 0) == 1;
			}
			return true;
		}
	}

	public static void SetHasShownValue(int value)
	{
		SettingsController.Instance.GeneralSettingsProvider.SetValue("first_open_first_launch_settings", value);
		SettingsController.Instance.GeneralSettingsProvider.SaveAll();
		PlayerPrefs.SetInt("first_open_first_launch_settings", value);
		PlayerPrefs.Save();
	}
}
