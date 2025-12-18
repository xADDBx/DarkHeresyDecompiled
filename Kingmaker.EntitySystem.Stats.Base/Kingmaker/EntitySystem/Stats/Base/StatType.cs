using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Stats.Base;

public enum StatType
{
	Unknown = 0,
	[FormerlySerializedAs("WarhammerBallisticSkill")]
	BallisticSkill = 1,
	[FormerlySerializedAs("WarhammerWeaponSkill")]
	WeaponSkill = 2,
	[FormerlySerializedAs("WarhammerStrength")]
	Strength = 3,
	[FormerlySerializedAs("WarhammerToughness")]
	Toughness = 4,
	[FormerlySerializedAs("WarhammerAgility")]
	Agility = 5,
	[FormerlySerializedAs("WarhammerIntelligence")]
	Intelligence = 6,
	[FormerlySerializedAs("WarhammerWillpower")]
	Willpower = 7,
	[FormerlySerializedAs("WarhammerPerception")]
	Perception = 8,
	[FormerlySerializedAs("WarhammerFellowship")]
	Fellowship = 9,
	SkillAthletics = 10,
	SkillTenacity = 11,
	SkillMobility = 12,
	SkillResistance = 13,
	SkillDemolition = 14,
	SkillReflexes = 15,
	SkillSleightOfHand = 16,
	SkillLoreHeresy = 17,
	SkillLoreXenos = 18,
	SkillLoreWarp = 19,
	SkillTechUse = 20,
	SkillInterrogation = 21,
	SkillMettle = 22,
	SkillAwareness = 23,
	SkillWits = 24,
	SkillIntimidation = 25,
	SkillDiplomacy = 26,
	SkillMedicae = 27,
	HitPoints = 28,
	Defence = 29,
	ArmorDamageReduction = 30,
	ArmorDurability = 31,
	Initiative = 33,
	[FormerlySerializedAs("WarhammerInitialAPBlue")]
	MovementPoints = 35,
	[FormerlySerializedAs("WarhammerInitialAPYellow")]
	ActionPoints = 36,
	Resolve = 37,
	MachineTrait = 38,
	CohesionRange = 39
}
