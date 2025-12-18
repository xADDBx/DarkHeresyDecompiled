namespace Kingmaker.Visual.Animation;

public static class WeaponTypeExtensions
{
	public static bool IsOneHanded(this WeaponType type)
	{
		return WeaponAnimationStyleHelper.IsOneHandedWeapon(type);
	}

	public static bool IsTwoHanded(this WeaponType type)
	{
		return WeaponAnimationStyleHelper.IsTwoHandedWeapon(type);
	}

	public static bool IsMelee(this WeaponType type)
	{
		return WeaponAnimationStyleHelper.IsMeleeWeapon(type);
	}

	public static bool IsRanged(this WeaponType type)
	{
		return WeaponAnimationStyleHelper.IsRangedWeapon(type);
	}
}
