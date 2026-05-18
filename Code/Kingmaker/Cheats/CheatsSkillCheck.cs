using Core.Cheats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Cheats;

public static class CheatsSkillCheck
{
	[Cheat(Name = "skill_check_athletics", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckAthletics(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Athletics, difficulty);
	}

	[Cheat(Name = "skill_check_awareness", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckAwareness(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Awareness, difficulty);
	}

	[Cheat(Name = "skill_check_demolition", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckDemolition(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Demolition, difficulty);
	}

	[Cheat(Name = "skill_check_diplomacy", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckDiplomacy(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Diplomacy, difficulty);
	}

	[Cheat(Name = "skill_check_interrogation", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckInterrogation(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Interrogation, difficulty);
	}

	[Cheat(Name = "skill_check_intimidation", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckIntimidation(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Intimidation, difficulty);
	}

	[Cheat(Name = "skill_check_lore_heresy", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckLoreHeresy(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.LoreHeresy, difficulty);
	}

	[Cheat(Name = "skill_check_lore_xenos", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckLoreXenos(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.LoreXenos, difficulty);
	}

	[Cheat(Name = "skill_check_lore_warp", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckLoreWarp(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.LoreWarp, difficulty);
	}

	[Cheat(Name = "skill_check_medicae", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckMedicae(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Medicae, difficulty);
	}

	[Cheat(Name = "skill_check_mettle", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckMettle(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Mettle, difficulty);
	}

	[Cheat(Name = "skill_check_mobility", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckMobility(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Mobility, difficulty);
	}

	[Cheat(Name = "skill_check_reflexes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckReflexes(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Reflexes, difficulty);
	}

	[Cheat(Name = "skill_check_resistance", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckResistance(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Resistance, difficulty);
	}

	[Cheat(Name = "skill_check_sleight_of_hand", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckSleightOfHand(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.SleightOfHand, difficulty);
	}

	[Cheat(Name = "skill_check_tech_use", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckTechUse(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.TechUse, difficulty);
	}

	[Cheat(Name = "skill_check_tenacity", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckTenacity(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Tenacity, difficulty);
	}

	[Cheat(Name = "skill_check_wits", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkillCheckWits(int difficulty = 0)
	{
		TriggerSkillCheck(SkillType.Wits, difficulty);
	}

	private static void TriggerSkillCheck(SkillType skill, int difficulty)
	{
		Rulebook.Trigger(new RulePerformSkillCheck(Utilities.GetUnitForCheat(), skill, difficulty));
	}
}
