using Kingmaker.Code.Framework.Settings.UISettings;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchLanguagePageVM : FirstLaunchSettingsPageVM
{
	public readonly FirstLaunchEntityLanguageVM Languages;

	public FirstLaunchLanguagePageVM()
	{
		Languages = new FirstLaunchEntityLanguageVM(UISettingsRoot.Instance.UIGameMainSettings.Localization, forceSetValue: true);
	}

	protected override void OnDispose()
	{
		Languages?.Dispose();
	}
}
