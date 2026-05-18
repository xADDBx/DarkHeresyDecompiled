using System;

namespace Kingmaker.UI.AR;

[Flags]
public enum CombatHudAreas : uint
{
	Walkable = 1u,
	Movement = 2u,
	ActiveUnit = 4u,
	Threatening = 8u,
	AbilityMinRange = 0x10u,
	AbilityMaxRange = 0x20u,
	DebugAlly = 0x40u,
	AbilityPrimary = 0x80u,
	AbilitySecondary = 0x100u,
	Cohesion = 0x200u,
	DebugHostile = 0x800u,
	Harmful = 0x1000u,
	LosBlocker = 0x2000u,
	CohesionIntersection = 0x4000u,
	ChannelingAbility = 0x10000u
}
