using System;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityDropdownGameDifficultyItemVM : ViewModel
{
	public readonly Sprite Icon;

	public readonly string Title;

	public readonly string Description;

	public readonly bool IsCustom;

	private readonly bool m_IsVeryHard;

	private readonly int m_Index;

	private readonly Action<int> m_SetSelected;

	public readonly ReadOnlyReactiveProperty<bool> IsSelected;

	private bool NeedWarning
	{
		get
		{
			if (m_IsVeryHard)
			{
				if ((GameDifficultyOption)SettingsRoot.Difficulty.GameDifficulty == GameDifficultyOption.Hard)
				{
					return (GameDifficultyOption)SettingsRoot.Difficulty.GameDifficulty != GameDifficultyOption.Unfair;
				}
				return true;
			}
			return false;
		}
	}

	public SettingsEntityDropdownGameDifficultyItemVM(DifficultyPresetAsset difficulty, int index, Action<int> setSelected, Observable<int> selectedIndex)
	{
		m_Index = index;
		m_SetSelected = setSelected;
		IsSelected = selectedIndex.Select((int i) => i == m_Index).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
		Title = difficulty.LocalizedName;
		Description = difficulty.LocalizedDescription;
		Icon = difficulty.Icon;
		IsCustom = difficulty.IsCustomMode;
		m_IsVeryHard = difficulty.Preset.GameDifficulty == GameDifficultyOption.Hard || difficulty.Preset.GameDifficulty == GameDifficultyOption.Unfair;
	}

	public void SetSelected()
	{
		m_SetSelected?.Invoke(m_Index);
	}
}
