using System;
using System.Collections.Generic;

namespace Kingmaker.Visual.Animation;

public static class WeaponAnimationStyleHelper
{
	private static readonly List<WeaponAnimationStyle> s_DualWieldingStyles = new List<WeaponAnimationStyle>
	{
		WeaponAnimationStyle.MeleeMelee,
		WeaponAnimationStyle.RangedRanged,
		WeaponAnimationStyle.MeleeRanged,
		WeaponAnimationStyle.RangedMelee,
		WeaponAnimationStyle.MeleeShield,
		WeaponAnimationStyle.RangedShield
	};

	private static readonly List<WeaponType> s_OneHandedWeapons = new List<WeaponType>
	{
		WeaponType.Fist,
		WeaponType.Knife,
		WeaponType.OneHandedSword,
		WeaponType.OneHandedAxe,
		WeaponType.OneHandedHammer,
		WeaponType.PistolBase,
		WeaponType.PistolFlamer,
		WeaponType.PistolLaser,
		WeaponType.PistolPlasma,
		WeaponType.Shield
	};

	private static readonly List<WeaponType> s_OneHandedMeleeWeapons = new List<WeaponType>
	{
		WeaponType.Fist,
		WeaponType.Knife,
		WeaponType.OneHandedSword,
		WeaponType.OneHandedAxe,
		WeaponType.OneHandedHammer
	};

	private static readonly List<WeaponType> s_OneHandedRangedWeapons = new List<WeaponType>
	{
		WeaponType.PistolBase,
		WeaponType.PistolFlamer,
		WeaponType.PistolLaser,
		WeaponType.PistolPlasma
	};

	private static readonly List<WeaponType> s_OneHandedShields = new List<WeaponType> { WeaponType.Shield };

	private static readonly List<WeaponType> s_Polearms = new List<WeaponType>
	{
		WeaponType.TwoHandedAxe,
		WeaponType.TwoHandedHammer
	};

	private static readonly List<WeaponType> s_TwoHandedSwords = new List<WeaponType> { WeaponType.TwoHandedSword };

	private static readonly List<WeaponType> s_Staffs = new List<WeaponType> { WeaponType.Staff };

	private static readonly List<WeaponType> s_Rifles = new List<WeaponType>
	{
		WeaponType.RifleHipBase,
		WeaponType.RifleHipFlamer,
		WeaponType.RifleHipPlasma,
		WeaponType.RifleShoulderBase,
		WeaponType.RifleShoulderLaser
	};

	private static readonly List<WeaponType> s_HeavyOnHipWeapons = new List<WeaponType>
	{
		WeaponType.HeavyHipBase,
		WeaponType.HeavyHipFlamer,
		WeaponType.HeavyHipLaser,
		WeaponType.HeavyHipPlasma
	};

	private static readonly List<WeaponType> s_HeavyOnShoulderWeapons = new List<WeaponType> { WeaponType.HeavyShoulder };

	private static readonly List<WeaponType> s_EldarRifles = new List<WeaponType>
	{
		WeaponType.EldarRifleHipBase,
		WeaponType.EldarRifleHipLaser,
		WeaponType.EldarRifleShoulderBase,
		WeaponType.EldarRifleShoulderLaser
	};

	private static readonly List<WeaponType> s_EldarHeavyOnHipWeapons = new List<WeaponType>
	{
		WeaponType.EldarHeavyHipBase,
		WeaponType.EldarHeavyHipLaser
	};

	private static readonly List<WeaponType> s_EldarHeavyOnShoulderWeapons = new List<WeaponType> { WeaponType.EldarHeavyShoulder };

	private static readonly List<WeaponType> s_CreatureWeapons = new List<WeaponType> { WeaponType.Creature };

	private static readonly List<WeaponAnimationStyle> s_DualWieldingStylesWithMelee = new List<WeaponAnimationStyle>
	{
		WeaponAnimationStyle.MeleeMelee,
		WeaponAnimationStyle.MeleeRanged,
		WeaponAnimationStyle.RangedMelee,
		WeaponAnimationStyle.MeleeShield
	};

	private static readonly List<WeaponAnimationStyle> s_DualWieldingStylesWithRanged = new List<WeaponAnimationStyle>
	{
		WeaponAnimationStyle.RangedRanged,
		WeaponAnimationStyle.MeleeRanged,
		WeaponAnimationStyle.RangedMelee,
		WeaponAnimationStyle.RangedShield
	};

	private static readonly List<WeaponAnimationStyle> s_DualWieldingStylesWithShield = new List<WeaponAnimationStyle>
	{
		WeaponAnimationStyle.MeleeShield,
		WeaponAnimationStyle.RangedShield
	};

	private static readonly Dictionary<WeaponAnimationStyle, (List<WeaponType> MainHand, List<WeaponType> OffHand)> s_StyleToTypeList = new Dictionary<WeaponAnimationStyle, (List<WeaponType>, List<WeaponType>)>
	{
		{
			WeaponAnimationStyle.MeleeMelee,
			(s_OneHandedMeleeWeapons, s_OneHandedMeleeWeapons)
		},
		{
			WeaponAnimationStyle.RangedRanged,
			(s_OneHandedRangedWeapons, s_OneHandedRangedWeapons)
		},
		{
			WeaponAnimationStyle.MeleeRanged,
			(s_OneHandedMeleeWeapons, s_OneHandedRangedWeapons)
		},
		{
			WeaponAnimationStyle.RangedMelee,
			(s_OneHandedRangedWeapons, s_OneHandedMeleeWeapons)
		},
		{
			WeaponAnimationStyle.MeleeShield,
			(s_OneHandedMeleeWeapons, s_OneHandedShields)
		},
		{
			WeaponAnimationStyle.RangedShield,
			(s_OneHandedRangedWeapons, s_OneHandedShields)
		},
		{
			WeaponAnimationStyle.Polearm,
			(s_Polearms, null)
		},
		{
			WeaponAnimationStyle.TwoHandedSword,
			(s_TwoHandedSwords, null)
		},
		{
			WeaponAnimationStyle.Staff,
			(s_Staffs, null)
		},
		{
			WeaponAnimationStyle.Rifle,
			(s_Rifles, null)
		},
		{
			WeaponAnimationStyle.HeavyHip,
			(s_HeavyOnHipWeapons, null)
		},
		{
			WeaponAnimationStyle.HeavyShoulder,
			(s_HeavyOnShoulderWeapons, null)
		},
		{
			WeaponAnimationStyle.EldarRifle,
			(s_EldarRifles, null)
		},
		{
			WeaponAnimationStyle.EldarHeavyHip,
			(s_EldarHeavyOnHipWeapons, null)
		},
		{
			WeaponAnimationStyle.EldarHeavyShoulder,
			(s_EldarHeavyOnShoulderWeapons, null)
		}
	};

	private static readonly Dictionary<WeaponType, List<WeaponAnimationStyle>> s_TypeToDualWieldingStyles = new Dictionary<WeaponType, List<WeaponAnimationStyle>>
	{
		{
			WeaponType.Fist,
			s_DualWieldingStylesWithMelee
		},
		{
			WeaponType.Knife,
			s_DualWieldingStylesWithMelee
		},
		{
			WeaponType.OneHandedSword,
			s_DualWieldingStylesWithMelee
		},
		{
			WeaponType.OneHandedAxe,
			s_DualWieldingStylesWithMelee
		},
		{
			WeaponType.OneHandedHammer,
			s_DualWieldingStylesWithMelee
		},
		{
			WeaponType.PistolBase,
			s_DualWieldingStylesWithRanged
		},
		{
			WeaponType.PistolFlamer,
			s_DualWieldingStylesWithRanged
		},
		{
			WeaponType.PistolLaser,
			s_DualWieldingStylesWithRanged
		},
		{
			WeaponType.PistolPlasma,
			s_DualWieldingStylesWithRanged
		},
		{
			WeaponType.Shield,
			s_DualWieldingStylesWithShield
		}
	};

	private static readonly Dictionary<WeaponType, WeaponAnimationStyle> s_TypeToTwoHandedStyles = new Dictionary<WeaponType, WeaponAnimationStyle>
	{
		{
			WeaponType.TwoHandedAxe,
			WeaponAnimationStyle.Polearm
		},
		{
			WeaponType.TwoHandedHammer,
			WeaponAnimationStyle.Polearm
		},
		{
			WeaponType.TwoHandedSword,
			WeaponAnimationStyle.TwoHandedSword
		},
		{
			WeaponType.Staff,
			WeaponAnimationStyle.Staff
		},
		{
			WeaponType.RifleHipBase,
			WeaponAnimationStyle.Rifle
		},
		{
			WeaponType.RifleHipFlamer,
			WeaponAnimationStyle.Rifle
		},
		{
			WeaponType.RifleHipPlasma,
			WeaponAnimationStyle.Rifle
		},
		{
			WeaponType.RifleShoulderBase,
			WeaponAnimationStyle.Rifle
		},
		{
			WeaponType.RifleShoulderLaser,
			WeaponAnimationStyle.Rifle
		},
		{
			WeaponType.HeavyHipBase,
			WeaponAnimationStyle.HeavyHip
		},
		{
			WeaponType.HeavyHipFlamer,
			WeaponAnimationStyle.HeavyHip
		},
		{
			WeaponType.HeavyHipLaser,
			WeaponAnimationStyle.HeavyHip
		},
		{
			WeaponType.HeavyHipPlasma,
			WeaponAnimationStyle.HeavyHip
		},
		{
			WeaponType.HeavyShoulder,
			WeaponAnimationStyle.HeavyShoulder
		},
		{
			WeaponType.EldarRifleHipBase,
			WeaponAnimationStyle.EldarRifle
		},
		{
			WeaponType.EldarRifleHipLaser,
			WeaponAnimationStyle.EldarRifle
		},
		{
			WeaponType.EldarRifleShoulderBase,
			WeaponAnimationStyle.EldarRifle
		},
		{
			WeaponType.EldarRifleShoulderLaser,
			WeaponAnimationStyle.EldarRifle
		},
		{
			WeaponType.EldarHeavyHipBase,
			WeaponAnimationStyle.EldarHeavyHip
		},
		{
			WeaponType.EldarHeavyHipLaser,
			WeaponAnimationStyle.EldarHeavyHip
		},
		{
			WeaponType.EldarHeavyShoulder,
			WeaponAnimationStyle.EldarHeavyShoulder
		},
		{
			WeaponType.Creature,
			WeaponAnimationStyle.Creature
		}
	};

	public static bool IsDualWieldingStyle(WeaponAnimationStyle style)
	{
		return s_DualWieldingStyles.Contains(style);
	}

	public static bool IsTwoHandedStyle(WeaponAnimationStyle style)
	{
		return !IsDualWieldingStyle(style);
	}

	public static bool IsOneHandedWeapon(WeaponType weapon)
	{
		return s_OneHandedWeapons.Contains(weapon);
	}

	public static bool IsTwoHandedWeapon(WeaponType weapon)
	{
		return !IsOneHandedWeapon(weapon);
	}

	public static bool IsMeleeWeapon(WeaponType weapon)
	{
		if (!s_OneHandedMeleeWeapons.Contains(weapon) && !s_TwoHandedSwords.Contains(weapon) && !s_Polearms.Contains(weapon))
		{
			return s_Staffs.Contains(weapon);
		}
		return true;
	}

	public static bool IsRangedWeapon(WeaponType weapon)
	{
		if (!s_OneHandedRangedWeapons.Contains(weapon) && !s_Rifles.Contains(weapon) && !s_HeavyOnHipWeapons.Contains(weapon) && !s_HeavyOnShoulderWeapons.Contains(weapon) && !s_EldarRifles.Contains(weapon) && !s_EldarHeavyOnHipWeapons.Contains(weapon))
		{
			return s_EldarHeavyOnShoulderWeapons.Contains(weapon);
		}
		return true;
	}

	public static (IReadOnlyList<WeaponType> MainHand, IReadOnlyList<WeaponType> OffHand) GetWeaponTypes(WeaponAnimationStyle style)
	{
		(List<WeaponType>, List<WeaponType>) tuple = s_StyleToTypeList[style];
		return (MainHand: tuple.Item1, OffHand: tuple.Item2);
	}

	public static WeaponAnimationStyle DetectTwoHandedWeaponStyle(WeaponType weapon)
	{
		if (!IsTwoHandedWeapon(weapon))
		{
			throw new ArgumentException($"{weapon} is not two-handed weapon");
		}
		return s_TypeToTwoHandedStyles[weapon];
	}

	public static WeaponAnimationStyle DetectDualWieldingStyle(WeaponType mainHandWeapon, WeaponType offHandWeapon)
	{
		if (!IsOneHandedWeapon(mainHandWeapon))
		{
			throw new ArgumentException($"{mainHandWeapon} is not one-handed weapon");
		}
		if (!IsOneHandedWeapon(offHandWeapon))
		{
			throw new ArgumentException($"{offHandWeapon} is not one-handed weapon");
		}
		if (s_OneHandedMeleeWeapons.Contains(mainHandWeapon))
		{
			if (s_OneHandedMeleeWeapons.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.MeleeMelee;
			}
			if (s_OneHandedRangedWeapons.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.MeleeRanged;
			}
			if (s_OneHandedShields.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.MeleeShield;
			}
			throw new Exception($"Can't find dual wielding weapon style for {mainHandWeapon} + {offHandWeapon}");
		}
		if (s_OneHandedRangedWeapons.Contains(mainHandWeapon))
		{
			if (s_OneHandedMeleeWeapons.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.RangedMelee;
			}
			if (s_OneHandedRangedWeapons.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.RangedRanged;
			}
			if (s_OneHandedShields.Contains(offHandWeapon))
			{
				return WeaponAnimationStyle.RangedShield;
			}
			throw new Exception($"Can't find dual wielding weapon style for {mainHandWeapon} + {offHandWeapon}");
		}
		throw new Exception($"Can't find dual wielding weapon style for {mainHandWeapon} + {offHandWeapon}");
	}
}
