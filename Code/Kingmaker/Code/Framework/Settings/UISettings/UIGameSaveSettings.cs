using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIGameSaveSettings : IUISettingsSheet
{
	public UISettingsEntityBool AutosaveEnabled;

	public UISettingsEntitySliderInt AutosaveSlots;

	public UISettingsEntitySliderInt QuicksaveSlots;

	public void LinkToSettings()
	{
		AutosaveEnabled.LinkSetting(SettingsRoot.Game.Save.AutosaveEnabled);
		AutosaveSlots.LinkSetting(SettingsRoot.Game.Save.AutosaveSlots);
		QuicksaveSlots.LinkSetting(SettingsRoot.Game.Save.QuicksaveSlots);
	}

	public void InitializeSettings()
	{
	}
}
