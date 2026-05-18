using System;

namespace Kingmaker.UnitLogic.Abilities;

public sealed record DamagePredictionData
{
	public int MinDamage;

	public int MaxDamage;

	public int HPDamageBonus;

	public int ArmorDamageBonus;

	public int VitalDamage;

	public static DamagePredictionData operator +(DamagePredictionData lhs, DamagePredictionData rhs)
	{
		if (lhs == null)
		{
			return rhs ?? new DamagePredictionData();
		}
		if (rhs == null)
		{
			return lhs;
		}
		return new DamagePredictionData
		{
			MinDamage = lhs.MinDamage + rhs.MinDamage,
			MaxDamage = lhs.MaxDamage + rhs.MaxDamage,
			HPDamageBonus = lhs.HPDamageBonus + rhs.HPDamageBonus,
			ArmorDamageBonus = lhs.ArmorDamageBonus + rhs.ArmorDamageBonus,
			VitalDamage = lhs.VitalDamage + rhs.VitalDamage
		};
	}

	public static DamagePredictionData Merge(DamagePredictionData lhs, DamagePredictionData rhs)
	{
		if (lhs == null)
		{
			return rhs ?? new DamagePredictionData();
		}
		if (rhs == null)
		{
			return lhs;
		}
		return new DamagePredictionData
		{
			MinDamage = Math.Min(lhs.MinDamage, rhs.MinDamage),
			MaxDamage = Math.Max(lhs.MaxDamage, rhs.MaxDamage),
			HPDamageBonus = Math.Max(lhs.HPDamageBonus, rhs.HPDamageBonus),
			ArmorDamageBonus = Math.Max(lhs.ArmorDamageBonus, rhs.ArmorDamageBonus),
			VitalDamage = Math.Max(lhs.VitalDamage, rhs.VitalDamage)
		};
	}
}
