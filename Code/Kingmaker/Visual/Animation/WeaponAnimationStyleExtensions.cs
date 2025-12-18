using System.Collections.Generic;

namespace Kingmaker.Visual.Animation;

public static class WeaponAnimationStyleExtensions
{
	public static bool IsDualWielding(this WeaponAnimationStyle style)
	{
		return WeaponAnimationStyleHelper.IsDualWieldingStyle(style);
	}

	public static bool IsTwoHanded(this WeaponAnimationStyle style)
	{
		return WeaponAnimationStyleHelper.IsTwoHandedStyle(style);
	}

	public static (IReadOnlyList<WeaponType> MainHand, IReadOnlyList<WeaponType> OffHand) GetWeaponTypes(this WeaponAnimationStyle style)
	{
		return WeaponAnimationStyleHelper.GetWeaponTypes(style);
	}
}
