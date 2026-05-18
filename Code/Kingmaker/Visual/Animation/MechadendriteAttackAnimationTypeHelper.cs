using System.Collections.Generic;
using System.ComponentModel;

namespace Kingmaker.Visual.Animation;

public static class MechadendriteAttackAnimationTypeHelper
{
	private static readonly List<WeaponType> s_BaseWeapons = new List<WeaponType>
	{
		WeaponType.PistolBase,
		WeaponType.RifleHipBase,
		WeaponType.RifleShoulderBase,
		WeaponType.HeavyHipBase,
		WeaponType.HeavyShoulder,
		WeaponType.EldarRifleHipBase,
		WeaponType.EldarRifleShoulderBase,
		WeaponType.EldarHeavyHipBase,
		WeaponType.EldarHeavyShoulder
	};

	private static readonly List<WeaponType> s_FlamerWeapons = new List<WeaponType>
	{
		WeaponType.PistolFlamer,
		WeaponType.RifleHipFlamer,
		WeaponType.HeavyHipFlamer
	};

	private static readonly List<WeaponType> s_LaserWeapons = new List<WeaponType>
	{
		WeaponType.PistolLaser,
		WeaponType.RifleShoulderLaser,
		WeaponType.HeavyHipLaser,
		WeaponType.EldarRifleHipLaser,
		WeaponType.EldarRifleShoulderLaser,
		WeaponType.EldarHeavyHipLaser
	};

	private static readonly List<WeaponType> s_PlasmaWeapons = new List<WeaponType>
	{
		WeaponType.PistolPlasma,
		WeaponType.RifleHipPlasma,
		WeaponType.HeavyHipPlasma
	};

	public static MechadendriteAttackAnimationType GetMechadendriteAttackAnimationType(this WeaponType weaponType)
	{
		if (!weaponType.IsRanged())
		{
			throw new InvalidEnumArgumentException($"Expected ranged weapon, but got {weaponType}");
		}
		if (s_BaseWeapons.Contains(weaponType))
		{
			return MechadendriteAttackAnimationType.MechadendriteBase;
		}
		if (s_FlamerWeapons.Contains(weaponType))
		{
			return MechadendriteAttackAnimationType.MechadendriteFlamer;
		}
		if (s_LaserWeapons.Contains(weaponType))
		{
			return MechadendriteAttackAnimationType.MechadendriteLaser;
		}
		if (s_PlasmaWeapons.Contains(weaponType))
		{
			return MechadendriteAttackAnimationType.MechadendritePlasma;
		}
		throw new InvalidEnumArgumentException($"Unknown ranged weapon: {weaponType}");
	}
}
