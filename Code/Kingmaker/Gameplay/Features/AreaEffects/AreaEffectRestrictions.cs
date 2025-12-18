using System;

namespace Kingmaker.Gameplay.Features.AreaEffects;

[Flags]
public enum AreaEffectRestrictions
{
	None = 1,
	CanOnlyUseWeaponAbilities = 2,
	CannotUseWeaponAbilities = 4,
	CannotUsePsychicPowers = 8
}
