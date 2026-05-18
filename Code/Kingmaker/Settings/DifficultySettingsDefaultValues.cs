using System;

namespace Kingmaker.Settings;

[Serializable]
public class DifficultySettingsDefaultValues
{
	public bool OnlyOneSave;

	public bool ImmersiveMode;

	public bool OnlyActiveCompanionsReceiveExperience;

	public bool OnlyInitiatorReceiveSkillCheckExperience;

	public bool LimitedAI;

	public GameDifficultyOption GameDifficulty;

	public EnemyDifficultyOption EnemyDurability;

	public EnemyDifficultyOption EnemyDamage;

	public int SkillCheckModifier;

	public int EnemyMovementPoints;

	public int EnemyDamageModifier;

	public int PartyDamageModifier;

	public int EnemyDodgeModifier;

	public int EnemySkillModifier;

	public int PartyPositiveMoraleChangeModifier;

	public int PartyNegativeMoraleChangeModifier;

	public int EnemyPositiveMoraleChangeModifier;

	public int EnemyNegativeMoraleChangeModifier;

	public int AllyResolveModifier;
}
