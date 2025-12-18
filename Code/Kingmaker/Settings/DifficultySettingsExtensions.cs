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
			EnemyDodgePercentModifier = settings.EnemyDodgePercentModifier,
			MinPartyDamage = settings.MinPartyDamage,
			MinPartyDamageFraction = settings.MinPartyDamageFraction,
			NPCAttributesBaseValuePercentModifier = settings.NPCAttributesBaseValuePercentModifier,
			HardCrowdControlOnPartyMaxDurationRounds = settings.HardCrowdControlOnPartyMaxDurationRounds,
			SkillCheckModifier = settings.SkillCheckModifier,
			EnemyHitPointsPercentModifier = settings.EnemyHitPointsPercentModifier,
			AllyResolveModifier = settings.AllyResolveModifier,
			PartyDamageDealtAfterArmorReductionPercentModifier = settings.PartyDamageDealtAfterArmorReductionPercentModifier,
			AvoidableDamagePercentModifier = settings.AvoidableDamagePercentModifier,
			EnemyMovementPoints = settings.EnemyMovementPoints,
			NPCDifficulty = settings.NPCDifficulty
		};
	}

	public static DifficultyPreset TempToDifficultyPreset(this DifficultySettings settings)
	{
		return new DifficultyPreset
		{
			GameDifficulty = settings.GameDifficulty.GetTempValue(),
			RespecAllowed = settings.RespecAllowed.GetTempValue(),
			CombatEncountersCapacity = settings.CombatEncountersCapacity.GetTempValue(),
			EnemyDodgePercentModifier = settings.EnemyDodgePercentModifier.GetTempValue(),
			MinPartyDamage = settings.MinPartyDamage.GetTempValue(),
			MinPartyDamageFraction = settings.MinPartyDamageFraction.GetTempValue(),
			NPCAttributesBaseValuePercentModifier = settings.NPCAttributesBaseValuePercentModifier.GetTempValue(),
			HardCrowdControlOnPartyMaxDurationRounds = settings.HardCrowdControlOnPartyMaxDurationRounds.GetTempValue(),
			SkillCheckModifier = settings.SkillCheckModifier.GetTempValue(),
			EnemyHitPointsPercentModifier = settings.EnemyHitPointsPercentModifier.GetTempValue(),
			AllyResolveModifier = settings.AllyResolveModifier.GetTempValue(),
			PartyDamageDealtAfterArmorReductionPercentModifier = settings.PartyDamageDealtAfterArmorReductionPercentModifier.GetTempValue(),
			AvoidableDamagePercentModifier = settings.AvoidableDamagePercentModifier.GetTempValue(),
			EnemyMovementPoints = settings.EnemyMovementPoints.GetTempValue(),
			NPCDifficulty = settings.NPCDifficulty.GetTempValue()
		};
	}
}
