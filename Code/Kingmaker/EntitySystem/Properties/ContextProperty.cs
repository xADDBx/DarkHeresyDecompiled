using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Properties;

public enum ContextProperty
{
	None = 0,
	BallisticSkill = 1,
	WeaponSkill = 2,
	Strength = 3,
	Toughness = 4,
	Agility = 5,
	Intelligence = 6,
	Willpower = 7,
	Perception = 8,
	Fellowship = 9,
	BallisticSkillBonus = 10,
	WeaponSkillBonus = 11,
	StrengthBonus = 12,
	ToughnessBonus = 13,
	AgilityBonus = 14,
	IntelligenceBonus = 15,
	WillpowerBonus = 16,
	PerceptionBonus = 17,
	FellowshipBonus = 18,
	[FormerlySerializedAs("InitialAPBlue")]
	MovementPoints = 19,
	[FormerlySerializedAs("InitialAPYellow")]
	ActionPoints = 20,
	CurrentWeaponRateOfFire = 21,
	EnemiesAdjacent = 22,
	[FormerlySerializedAs("CurrentAPBlue")]
	CurrentMovementPoints = 23,
	[FormerlySerializedAs("CurrentAPYellow")]
	CurrentActionPoints = 24,
	SkillAthletics = 25,
	SkillAwareness = 26,
	SkillDemolition = 29,
	SkillMedicae = 31,
	SkillLoreXenos = 32,
	SkillLoreWarp = 33,
	SkillTechUse = 35,
	SkillTenacity = 43,
	SkillMobility = 44,
	SkillResistance = 45,
	SkillReflexes = 46,
	SkillSleightOfHand = 47,
	SkillLoreHeresy = 48,
	SkillInterrogation = 49,
	SkillMettle = 50,
	SkillWits = 51,
	SkillIntimidation = 52,
	SkillDiplomacy = 53,
	Resolve = 40,
	Wounds = 41,
	ContextFactRank = 54
}
