using System;
using System.Collections.Generic;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIKeybindActionBarSettings : IUISettingsSheet
{
	public UISettingsEntityKeyBinding ChangeWeaponSet;

	public UISettingsEntityKeyBinding ChangeAbilitySet;

	public List<UISettingsEntityKeyBinding> ActionBarConsumables;

	public List<UISettingsEntityKeyBinding> ActionBarWeapons;

	public List<UISettingsEntityKeyBinding> ActionBarAbilities;

	public void LinkToSettings()
	{
		ChangeWeaponSet.LinkSetting(SettingsRoot.Controls.Keybindings.ActionBar.ChangeWeaponSet);
		ChangeAbilitySet.LinkSetting(SettingsRoot.Controls.Keybindings.ActionBar.ChangeAbilitySet);
		for (int i = 0; i < ActionBarConsumables.Count; i++)
		{
			ActionBarConsumables[i].LinkSetting(SettingsRoot.Controls.Keybindings.ActionBar.ActionBarConsumables[i]);
		}
		for (int j = 0; j < ActionBarWeapons.Count; j++)
		{
			ActionBarWeapons[j].LinkSetting(SettingsRoot.Controls.Keybindings.ActionBar.ActionBarWeapons[j]);
		}
		for (int k = 0; k < ActionBarAbilities.Count; k++)
		{
			ActionBarAbilities[k].LinkSetting(SettingsRoot.Controls.Keybindings.ActionBar.ActionBarAbilities[k]);
		}
	}

	public void InitializeSettings()
	{
	}
}
