using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIGameTooltipsSettings : IUISettingsSheet
{
	public UISettingsEntityBool ShowComparative;

	public UISettingsEntitySliderFloat ShowDelay;

	public UISettingsEntityBool Shortened;

	public void LinkToSettings()
	{
		ShowComparative.LinkSetting(SettingsRoot.Game.Tooltips.ShowComparative);
		ShowDelay.LinkSetting(SettingsRoot.Game.Tooltips.ShowDelay);
		Shortened.LinkSetting(SettingsRoot.Game.Tooltips.Shortened);
	}

	public void InitializeSettings()
	{
	}
}
