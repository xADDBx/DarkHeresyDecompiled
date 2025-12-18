using System;
using System.Linq;
using Kingmaker.Enums;
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

	public readonly SettingsEntityInt EnemyDodgePercentModifier;

	[Obsolete]
	public readonly SettingsEntityInt MinPartyDamage;

	[Obsolete]
	public readonly SettingsEntityInt MinPartyDamageFraction;

	public readonly SettingsEntityInt NPCAttributesBaseValuePercentModifier;

	public readonly SettingsEntityEnum<HardCrowdControlDurationLimit> HardCrowdControlOnPartyMaxDurationRounds;

	public readonly SettingsEntityInt SkillCheckModifier;

	public readonly SettingsEntityInt EnemyHitPointsPercentModifier;

	public readonly SettingsEntityInt AllyResolveModifier;

	public readonly SettingsEntityInt PartyDamageDealtAfterArmorReductionPercentModifier;

	public readonly SettingsEntityInt EnemyMovementPoints;

	public readonly SettingsEntityInt AvoidableDamagePercentModifier;

	public readonly SettingsEntityInt MinCRScaling;

	public readonly SettingsEntityInt MaxCRScaling;

	public readonly SettingsEntityEnum<NPCDifficultyOption> NPCDifficulty;

	private const int MinCRScalingFixed = 0;

	private const int MaxCRScalingFixed = 15;

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
		EnemyDodgePercentModifier = new SettingsEntityInt(settingsController, "enemy-dodge-percent-modifier", difficultyPreset.EnemyDodgePercentModifier, saveDependent: true);
		MinPartyDamage = new SettingsEntityInt(settingsController, "min-party-damage", difficultyPreset.MinPartyDamage, saveDependent: true);
		MinPartyDamageFraction = new SettingsEntityInt(settingsController, "min-party-damage-fraction", difficultyPreset.MinPartyDamageFraction, saveDependent: true);
		NPCAttributesBaseValuePercentModifier = new SettingsEntityInt(settingsController, "npc-attributes-base-value-percent-modifier", difficultyPreset.NPCAttributesBaseValuePercentModifier, saveDependent: true);
		HardCrowdControlOnPartyMaxDurationRounds = new SettingsEntityEnum<HardCrowdControlDurationLimit>(settingsController, "hard-crowd-control-on-party-max-duration-rounds", difficultyPreset.HardCrowdControlOnPartyMaxDurationRounds, saveDependent: true);
		SkillCheckModifier = new SettingsEntityInt(settingsController, "skill-check-modifier", difficultyPreset.SkillCheckModifier, saveDependent: true);
		EnemyHitPointsPercentModifier = new SettingsEntityInt(settingsController, "enemy-hit-points-percent-modifier", difficultyPreset.EnemyHitPointsPercentModifier, saveDependent: true);
		AllyResolveModifier = new SettingsEntityInt(settingsController, "party-resolve-modifier", difficultyPreset.AllyResolveModifier, saveDependent: true);
		PartyDamageDealtAfterArmorReductionPercentModifier = new SettingsEntityInt(settingsController, "party-damage-dealt-after-armor-reduction-percent-modifier", difficultyPreset.PartyDamageDealtAfterArmorReductionPercentModifier, saveDependent: true);
		AvoidableDamagePercentModifier = new SettingsEntityInt(settingsController, "avoidable-damage-percent-modifier", difficultyPreset.AvoidableDamagePercentModifier, saveDependent: true);
		EnemyMovementPoints = new SettingsEntityInt(settingsController, "enemy-movement-points", difficultyPreset.EnemyMovementPoints, saveDependent: true);
		MinCRScaling = new SettingsEntityInt(settingsController, "min-cr-for-scaling", 0, saveDependent: true);
		MaxCRScaling = new SettingsEntityInt(settingsController, "max-cr-for-scaling", 15, saveDependent: true);
		NPCDifficulty = new SettingsEntityEnum<NPCDifficultyOption>(settingsController, "npc-difficulty", difficultyPreset.NPCDifficulty, saveDependent: true);
	}
}
