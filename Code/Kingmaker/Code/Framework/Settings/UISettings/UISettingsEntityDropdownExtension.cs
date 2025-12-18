namespace Kingmaker.Code.Framework.Settings.UISettings;

public static class UISettingsEntityDropdownExtension
{
	public static int ValuesCount(this IUISettingsEntityDropdown settingsEntityDropdown)
	{
		return settingsEntityDropdown.LocalizedValues.Count;
	}
}
