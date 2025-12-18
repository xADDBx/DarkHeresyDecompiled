using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityDropdownGameDifficultyVM : SettingsEntityDropdownVM
{
	public readonly List<SettingsEntityDropdownGameDifficultyItemVM> Items;

	private readonly Action<int> m_ValueSetter;

	public SettingsEntityDropdownGameDifficultyVM(UISettingsEntityGameDifficulty uiSettingsEntity, bool forceSetValue = false, bool hideMarkImage = false)
		: base(uiSettingsEntity, DropdownType.Default, hideMarkImage)
	{
		Items = new List<SettingsEntityDropdownGameDifficultyItemVM>();
		for (int i = 0; i < ConfigRoot.Instance.DifficultyList.Difficulties.Count; i++)
		{
			DifficultyPresetAsset difficulty = ConfigRoot.Instance.DifficultyList.Difficulties[i];
			m_ValueSetter = (forceSetValue ? new Action<int>(base.SetValueAndConfirm) : new Action<int>(base.SetTempValue));
			SettingsEntityDropdownGameDifficultyItemVM settingsEntityDropdownGameDifficultyItemVM = new SettingsEntityDropdownGameDifficultyItemVM(difficulty, i, m_ValueSetter, TempIndexValue);
			AddDisposable(settingsEntityDropdownGameDifficultyItemVM);
			Items.Add(settingsEntityDropdownGameDifficultyItemVM);
		}
	}

	public void SetValue(int index)
	{
		m_ValueSetter?.Invoke(index);
	}
}
