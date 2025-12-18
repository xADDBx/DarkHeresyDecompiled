using System;

namespace Kingmaker.Enums;

public enum WeaponStat
{
	None,
	ShotHitChance,
	StrikeHitChance,
	PreciseHitChance,
	[Obsolete("WH2-7361")]
	AmmoCount,
	AttacksCount,
	AdditionalArmorDamage,
	AdditionalWoundsDamage,
	AdditionalVitalDamage
}
