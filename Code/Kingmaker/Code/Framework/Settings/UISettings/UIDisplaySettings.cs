using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIDisplaySettings : IUISettingsSheet
{
	public UISettingsEntityGammaCorrection GammaCorrection;

	public UISettingsEntityGammaCorrection Brightness;

	public UISettingsEntitySliderFloat Contrast;

	public UISettingsEntitySliderInt SafeZoneOffset;

	public void LinkToSettings()
	{
		GammaCorrection.LinkSetting(SettingsRoot.Display.GammaCorrection);
		Brightness.LinkSetting(SettingsRoot.Display.Brightness);
		Contrast.LinkSetting(SettingsRoot.Display.Contrast);
		SafeZoneOffset.LinkSetting(SettingsRoot.Display.SafeZoneOffset);
	}

	public void InitializeSettings()
	{
	}
}
