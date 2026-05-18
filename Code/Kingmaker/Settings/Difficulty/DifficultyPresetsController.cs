using System;
using System.Linq;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.DisposableExtension;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Settings.Difficulty;

public class DifficultyPresetsController
{
	private readonly DifficultySettings m_Settings;

	private readonly DifficultyPreset[] m_Presets;

	private DifficultyPreset m_LastSetValues;

	private readonly DisposableBooleanFlag m_ApplyingPreset = new DisposableBooleanFlag();

	private readonly int[] m_DifficultiesComparisonsWithCurrent;

	public DifficultyPresetsController(DifficultyPresetsList difficultyPresetsList)
	{
		m_Settings = SettingsRoot.Difficulty;
		AddTempValueChangedAdditionalMethod(delegate
		{
			OnTempSettingChanged();
		});
		m_Presets = difficultyPresetsList.Difficulties.Select((DifficultyPresetAsset blueprint) => blueprint.Preset).ToArray();
		m_Settings.GameDifficulty.OnTempValueChanged += SetPreset;
		ApplyCurrentDifficultyPreset();
		m_DifficultiesComparisonsWithCurrent = new int[difficultyPresetsList.Difficulties.Count()];
		UpdateGameDifficultiesComparisons();
	}

	private void SetPreset(GameDifficultyOption difficulty)
	{
		if (difficulty != GameDifficultyOption.Custom)
		{
			DifficultyPreset difficultyPreset = m_Presets.FindOrDefault((DifficultyPreset p) => p.GameDifficulty == difficulty);
			if (difficultyPreset != null)
			{
				SetValues(difficultyPreset);
			}
		}
	}

	public void SetDifficultyPreset(DifficultyPreset values, bool confirm)
	{
		SetValues(values);
		if (confirm)
		{
			SettingsController.Instance.ConfirmAllTempValues();
		}
		UpdateGameDifficultiesComparisons();
	}

	private void SetValues(DifficultyPreset values)
	{
		m_LastSetValues = values;
		using (m_ApplyingPreset.Retain())
		{
			m_Settings.GameDifficulty.SetTempValue(values.GameDifficulty);
			m_Settings.RespecAllowed.SetTempValue(values.RespecAllowed);
			m_Settings.CombatEncountersCapacity.SetTempValue(values.CombatEncountersCapacity);
			m_Settings.EnemyDurability.SetTempValue(values.EnemyDurability);
			m_Settings.EnemyDamage.SetTempValue(values.EnemyDamage);
			m_Settings.SkillCheckModifier.SetTempValue(values.SkillCheckModifier);
			m_Settings.EnemyMovementPoints.SetTempValue(values.EnemyMovementPoints);
			m_Settings.EnemyDamageModifier.SetTempValue(values.EnemyDamageModifier);
			m_Settings.PartyDamageModifier.SetTempValue(values.PartyDamageModifier);
			m_Settings.EnemyDodgeModifier.SetTempValue(values.EnemyDodgeModifier);
			m_Settings.EnemySkillModifier.SetTempValue(values.EnemySkillModifier);
			m_Settings.PartyPositiveMoraleChangeModifier.SetTempValue(values.PartyPositiveMoraleChangeModifier);
			m_Settings.PartyNegativeMoraleChangeModifier.SetTempValue(values.PartyNegativeMoraleChangeModifier);
			m_Settings.EnemyPositiveMoraleChangeModifier.SetTempValue(values.EnemyPositiveMoraleChangeModifier);
			m_Settings.EnemyNegativeMoraleChangeModifier.SetTempValue(values.EnemyNegativeMoraleChangeModifier);
			m_Settings.AllyResolveModifier.SetTempValue(values.AllyResolveModifier);
		}
	}

	private void AddTempValueChangedAdditionalMethod(Action method)
	{
		m_Settings.RespecAllowed.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.CombatEncountersCapacity.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyDurability.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyDamage.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.SkillCheckModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyMovementPoints.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyDamageModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.PartyDamageModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyDodgeModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemySkillModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.PartyPositiveMoraleChangeModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.PartyNegativeMoraleChangeModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyPositiveMoraleChangeModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyNegativeMoraleChangeModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.AllyResolveModifier.OnTempValueChanged += delegate
		{
			method();
		};
	}

	private void OnTempSettingChanged()
	{
		if (ContextData<BlueprintUnitCheckerInEditorContextData>.Current != null || (bool)m_ApplyingPreset || m_LastSetValues == null)
		{
			return;
		}
		if (m_LastSetValues.CompareTo(ExtractFromTempSettings()) == 0)
		{
			m_Settings.GameDifficulty.SetTempValue(m_LastSetValues.GameDifficulty);
			return;
		}
		m_Settings.GameDifficulty.SetTempValue(GameDifficultyOption.Custom);
		m_Presets.FindOrDefault((DifficultyPreset p) => p.GameDifficulty == GameDifficultyOption.Custom);
	}

	private DifficultyPreset ExtractFromSettings()
	{
		return m_Settings.ToDifficultyPreset();
	}

	private DifficultyPreset ExtractFromTempSettings()
	{
		return m_Settings.TempToDifficultyPreset();
	}

	public void UpdateGameDifficultiesComparisons()
	{
		DifficultyPreset difficultyPreset = ExtractFromSettings();
		for (int i = 0; i < m_DifficultiesComparisonsWithCurrent.Length; i++)
		{
			m_DifficultiesComparisonsWithCurrent[i] = difficultyPreset.CompareTo(m_Presets[i]);
		}
	}

	public void ApplyCurrentDifficultyPreset()
	{
		if ((GameDifficultyOption)m_Settings.GameDifficulty == GameDifficultyOption.Core)
		{
			m_Settings.GameDifficulty.SetValueAndConfirm(GameDifficultyOption.Custom);
		}
		if ((GameDifficultyOption)m_Settings.GameDifficulty != GameDifficultyOption.Custom)
		{
			SetPreset(m_Settings.GameDifficulty);
		}
	}

	public int CurrentDifficultyCompareTo(GameDifficultyOption gameDifficultyOption)
	{
		return m_DifficultiesComparisonsWithCurrent[EnumUtils.GetOrdinalNumber(gameDifficultyOption)];
	}
}
