using System.Collections.Generic;
using System.Linq;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Predictions;

public struct UIDamagePredictionData
{
	public int MinDamagePerAttack;

	public int MaxDamagePerAttack;

	public int BaseMinDamage;

	public int BaseMaxDamage;

	public IReadOnlyList<Modifier> DamageModifiers;

	public int ArmorMaxDamage;

	public int HealthMaxDamage;

	public int VitalDamage;

	public VitalDamageResult VitalDamageResult;

	public int HPDamageBonus;

	public int ArmorDamageBonus;

	public float DamageReduction;

	public int MaxDamage => ArmorMaxDamage + HealthMaxDamage;

	public bool Equals(UIDamagePredictionData other)
	{
		if (object.Equals(MinDamagePerAttack, other.MinDamagePerAttack) && object.Equals(MaxDamagePerAttack, other.MaxDamagePerAttack) && object.Equals(BaseMinDamage, other.BaseMinDamage) && object.Equals(BaseMaxDamage, other.BaseMaxDamage) && object.Equals(HPDamageBonus, other.HPDamageBonus) && object.Equals(ArmorDamageBonus, other.ArmorDamageBonus) && object.Equals(VitalDamage, other.VitalDamage) && object.Equals(VitalDamageResult, other.VitalDamageResult) && ModifiersEqual(other.DamageModifiers))
		{
			return object.Equals(DamageReduction, other.DamageReduction);
		}
		return false;
	}

	private bool ModifiersEqual(IReadOnlyList<Modifier> other)
	{
		if (DamageModifiers == other)
		{
			return true;
		}
		if (DamageModifiers == null || other == null)
		{
			return false;
		}
		return DamageModifiers.SequenceEqual(other);
	}
}
