using System;
using System.Collections.Generic;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIKeybindPreciseAttackSettings : IUISettingsSheet
{
	public List<UISettingsEntityKeyBinding> PreciseAttackBodyParts;

	public UISettingsEntityKeyBinding PreciseAttackConfirm;

	public UISettingsEntityKeyBinding PreciseAttackPrevTarget;

	public UISettingsEntityKeyBinding PreciseAttackNextTarget;

	public void LinkToSettings()
	{
		for (int i = 0; i < PreciseAttackBodyParts.Count; i++)
		{
			PreciseAttackBodyParts[i].LinkSetting(SettingsRoot.Controls.Keybindings.PreciseAttack.PreciseAttackBodyParts[i]);
		}
		PreciseAttackConfirm.LinkSetting(SettingsRoot.Controls.Keybindings.PreciseAttack.PreciseAttackConfirm);
		PreciseAttackPrevTarget.LinkSetting(SettingsRoot.Controls.Keybindings.PreciseAttack.PreciseAttackPrevTarget);
		PreciseAttackNextTarget.LinkSetting(SettingsRoot.Controls.Keybindings.PreciseAttack.PreciseAttackNextTarget);
	}

	public void InitializeSettings()
	{
	}
}
