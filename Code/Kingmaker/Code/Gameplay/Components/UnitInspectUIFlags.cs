using System;

namespace Kingmaker.Code.Gameplay.Components;

[Flags]
public enum UnitInspectUIFlags
{
	None = 0,
	HideUnitInfo = 1,
	HideCharacteristics = 2,
	HideMorale = 4,
	HideEffectsAndConditions = 8,
	HideCriticalEffects = 0x10,
	HideStatusEffects = 0x20,
	HideDotEffects = 0x40,
	HideNegativeEffects = 0x80,
	HidePositiveEffects = 0x100
}
