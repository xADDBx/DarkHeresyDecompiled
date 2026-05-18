using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Settings;

public static class DifficultySettingsExtensions
{
	public static DifficultyPreset ToDifficultyPreset(this DifficultySettings settings)
	{
		return new DifficultyPreset
		{
			GameDifficulty = settings.GameDifficulty,
			RespecAllowed = settings.RespecAllowed,
			CombatEncountersCapacity = settings.CombatEncountersCapacity,
			EnemyDurability = settings.EnemyDurability,
			EnemyDamage = settings.EnemyDamage,
			SkillCheckModifier = settings.SkillCheckModifier,
			EnemyMovementPoints = settings.EnemyMovementPoints,
			EnemyDamageModifier = settings.EnemyDamageModifier,
			PartyDamageModifier = settings.PartyDamageModifier,
			EnemyDodgeModifier = settings.EnemyDodgeModifier,
			EnemySkillModifier = settings.EnemySkillModifier,
			PartyPositiveMoraleChangeModifier = settings.PartyPositiveMoraleChangeModifier,
			PartyNegativeMoraleChangeModifier = settings.PartyNegativeMoraleChangeModifier,
			EnemyPositiveMoraleChangeModifier = settings.EnemyPositiveMoraleChangeModifier,
			EnemyNegativeMoraleChangeModifier = settings.EnemyNegativeMoraleChangeModifier,
			AllyResolveModifier = settings.AllyResolveModifier
		};
	}

	public static DifficultyPreset TempToDifficultyPreset(this DifficultySettings settings)
	{
		return new DifficultyPreset
		{
			GameDifficulty = settings.GameDifficulty.GetTempValue(),
			RespecAllowed = settings.RespecAllowed.GetTempValue(),
			CombatEncountersCapacity = settings.CombatEncountersCapacity.GetTempValue(),
			EnemyDurability = settings.EnemyDurability.GetTempValue(),
			EnemyDamage = settings.EnemyDamage.GetTempValue(),
			SkillCheckModifier = settings.SkillCheckModifier.GetTempValue(),
			EnemyMovementPoints = settings.EnemyMovementPoints.GetTempValue(),
			EnemyDamageModifier = settings.EnemyDamageModifier.GetTempValue(),
			PartyDamageModifier = settings.PartyDamageModifier.GetTempValue(),
			EnemyDodgeModifier = settings.EnemyDodgeModifier.GetTempValue(),
			EnemySkillModifier = settings.EnemySkillModifier.GetTempValue(),
			PartyPositiveMoraleChangeModifier = settings.PartyPositiveMoraleChangeModifier.GetTempValue(),
			PartyNegativeMoraleChangeModifier = settings.PartyNegativeMoraleChangeModifier.GetTempValue(),
			EnemyPositiveMoraleChangeModifier = settings.EnemyPositiveMoraleChangeModifier.GetTempValue(),
			EnemyNegativeMoraleChangeModifier = settings.EnemyNegativeMoraleChangeModifier.GetTempValue(),
			AllyResolveModifier = settings.AllyResolveModifier.GetTempValue()
		};
	}
}
