using System;
using System.Linq;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class DifficultySettings
{
	public readonly SettingsEntityEnum<GameDifficultyOption> GameDifficulty;

	public readonly SettingsEntityBool OnlyOneSave;

	public readonly SettingsEntityEnum<CombatEncountersCapacity> CombatEncountersCapacity;

	public readonly SettingsEntityBool RespecAllowed;

	public readonly SettingsEntityEnum<EnemyDifficultyOption> EnemyDurability;

	public readonly SettingsEntityEnum<EnemyDifficultyOption> EnemyDamage;

	public readonly SettingsEntityInt SkillCheckModifier;

	public readonly SettingsEntityInt EnemyMovementPoints;

	public readonly SettingsEntityInt EnemyDamageModifier;

	public readonly SettingsEntityInt PartyDamageModifier;

	public readonly SettingsEntityInt EnemyDodgeModifier;

	public readonly SettingsEntityInt EnemySkillModifier;

	public readonly SettingsEntityInt PartyPositiveMoraleChangeModifier;

	public readonly SettingsEntityInt PartyNegativeMoraleChangeModifier;

	public readonly SettingsEntityInt EnemyPositiveMoraleChangeModifier;

	public readonly SettingsEntityInt EnemyNegativeMoraleChangeModifier;

	public readonly SettingsEntityInt AllyResolveModifier;

	public DifficultySettings(ISettingsController settingsController, SettingsValues settingsValues)
	{
		DifficultySettingsDefaultValues defaultValues = settingsValues.SettingsDefaultValues.Difficulty;
		DifficultyPreset difficultyPreset = settingsValues.DifficultiesPresets.Difficulties.FirstOrDefault((DifficultyPresetAsset p) => p.Preset.GameDifficulty == defaultValues.GameDifficulty)?.Preset;
		if (difficultyPreset == null)
		{
			throw new Exception($"DifficultySettings: couldn't find {defaultValues.GameDifficulty} preset in current SettingsDefaultValues");
		}
		GameDifficulty = new SettingsEntityEnum<GameDifficultyOption>(settingsController, "game-difficulty", difficultyPreset.GameDifficulty, saveDependent: true);
		OnlyOneSave = new SettingsEntityBool(settingsController, "only-one-save", defaultValues.OnlyOneSave, saveDependent: true);
		CombatEncountersCapacity = new SettingsEntityEnum<CombatEncountersCapacity>(settingsController, "combat-encounters-capacity", difficultyPreset.CombatEncountersCapacity, saveDependent: true);
		RespecAllowed = new SettingsEntityBool(settingsController, "respec-allowed", difficultyPreset.RespecAllowed, saveDependent: true);
		EnemyDurability = new SettingsEntityEnum<EnemyDifficultyOption>(settingsController, "enemy-durability", difficultyPreset.EnemyDurability, saveDependent: true);
		EnemyDamage = new SettingsEntityEnum<EnemyDifficultyOption>(settingsController, "enemy-damage", difficultyPreset.EnemyDamage, saveDependent: true);
		SkillCheckModifier = new SettingsEntityInt(settingsController, "skill-check-modifier", difficultyPreset.SkillCheckModifier, saveDependent: true);
		EnemyMovementPoints = new SettingsEntityInt(settingsController, "enemy-movement-points", difficultyPreset.EnemyMovementPoints, saveDependent: true);
		EnemyDamageModifier = new SettingsEntityInt(settingsController, "enemy-damage-modifier", difficultyPreset.EnemyDamageModifier, saveDependent: true);
		PartyDamageModifier = new SettingsEntityInt(settingsController, "party-damage-modifier", difficultyPreset.PartyDamageModifier, saveDependent: true);
		EnemyDodgeModifier = new SettingsEntityInt(settingsController, "enemy-dodge-modifier", difficultyPreset.EnemyDodgeModifier, saveDependent: true);
		EnemySkillModifier = new SettingsEntityInt(settingsController, "enemy-skill-modifier", difficultyPreset.EnemySkillModifier, saveDependent: true);
		PartyPositiveMoraleChangeModifier = new SettingsEntityInt(settingsController, "party-positive-morale-change-modifier", difficultyPreset.PartyPositiveMoraleChangeModifier, saveDependent: true);
		PartyNegativeMoraleChangeModifier = new SettingsEntityInt(settingsController, "party-negative-morale-change-modifier", difficultyPreset.PartyNegativeMoraleChangeModifier, saveDependent: true);
		EnemyPositiveMoraleChangeModifier = new SettingsEntityInt(settingsController, "enemy-positive-morale-change-modifier", difficultyPreset.EnemyPositiveMoraleChangeModifier, saveDependent: true);
		EnemyNegativeMoraleChangeModifier = new SettingsEntityInt(settingsController, "enemy-negative-morale-change-modifier", difficultyPreset.EnemyNegativeMoraleChangeModifier, saveDependent: true);
		AllyResolveModifier = new SettingsEntityInt(settingsController, "party-resolve-modifier", difficultyPreset.AllyResolveModifier, saveDependent: true);
	}
}
