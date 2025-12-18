namespace Kingmaker.UnitLogic.Abilities;

public struct UIDamagePredictionData
{
	public int MinDamagePerAttack;

	public int MaxDamagePerAttack;

	public int BaseMinDamage;

	public int BaseMaxDamage;

	public int BaseDamageModifiers;

	public int ArmorMaxDamage;

	public int HealthMaxDamage;

	public bool VitalDamageLockedByArmor;

	public bool VitalDamageLockedByStrategy;

	public int VitalDamage;

	public int HPDamageBonus;

	public int ArmorDamageBonus;

	public float DamageReduction;

	public bool Equals(UIDamagePredictionData other)
	{
		if (object.Equals(MinDamagePerAttack, other.MinDamagePerAttack) && object.Equals(MaxDamagePerAttack, other.MaxDamagePerAttack) && object.Equals(BaseMinDamage, other.BaseMinDamage) && object.Equals(BaseMaxDamage, other.BaseMaxDamage) && object.Equals(HPDamageBonus, other.HPDamageBonus) && object.Equals(ArmorDamageBonus, other.ArmorDamageBonus) && object.Equals(VitalDamage, other.VitalDamage) && object.Equals(VitalDamageLockedByArmor, other.VitalDamageLockedByArmor) && object.Equals(VitalDamageLockedByStrategy, other.VitalDamageLockedByStrategy))
		{
			return object.Equals(DamageReduction, other.DamageReduction);
		}
		return false;
	}
}
