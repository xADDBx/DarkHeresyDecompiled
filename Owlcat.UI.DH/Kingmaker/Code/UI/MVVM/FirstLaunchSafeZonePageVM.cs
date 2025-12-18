using Kingmaker.Code.Framework.Settings.UISettings;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSafeZonePageVM : FirstLaunchSettingsPageVM
{
	public readonly SettingsEntitySliderVM Offset;

	public FirstLaunchSafeZonePageVM()
	{
		Offset = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIDisplaySettings.SafeZoneOffset);
	}
}
