using System;

namespace Kingmaker.Code.Gameplay.Components;

[Flags]
public enum UnitOvertipUIPart
{
	None = 0,
	Name = 1,
	HealthBar = 2,
	HitChance = 4,
	Damage = 8,
	Cover = 0x10,
	Buffs = 0x20,
	SpecialBuffs = 0x40,
	BuffPrediction = 0x80,
	Concentration = 0x100,
	Morale = 0x200,
	CombatText = 0x400
}
