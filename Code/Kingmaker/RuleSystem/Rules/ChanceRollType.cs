using System;

namespace Kingmaker.RuleSystem.Rules;

[Flags]
public enum ChanceRollType
{
	Defence = 1,
	Attack = 2,
	SkillCheck = 4,
	AvoidDeath = 8,
	NotSpendConsumable = 0x10,
	[Obsolete]
	Skill = 0x20,
	[Obsolete]
	Attribute = 0x40
}
