using System;

namespace Kingmaker.Code.Gameplay.Components;

[Flags]
public enum CombatTextTargetType
{
	None = 0,
	Enemy = 1,
	Ally = 2,
	NPC = 4,
	MapObject = 8,
	Personal = 0x10,
	All = -1
}
