using System;

namespace Kingmaker.UI.AR;

[Flags]
public enum CombatHudAreas : ushort
{
	Walkable = 1,
	Movement = 2,
	ActiveUnit = 4,
	Threatening = 8,
	AbilityMinRange = 0x10,
	AbilityMaxRange = 0x20,
	DebugAlly = 0x40,
	AbilityPrimary = 0x80,
	AbilitySecondary = 0x100,
	CohesionAlly = 0x200,
	CohesionHostile = 0x400,
	DebugHostile = 0x800,
	LosBlocker = 0x2000,
	CohesionAllyIntersection = 0x4000,
	CohesionHostileIntersection = 0x8000
}
