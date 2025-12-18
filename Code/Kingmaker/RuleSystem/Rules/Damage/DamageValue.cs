using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

public struct DamageValue
{
	public readonly IntermediateDamage Source;

	public readonly int RolledValue;

	public readonly int Reduction;

	public readonly int ArmorValue;

	public readonly int BonusArmorValue;

	public readonly int HealthValue;

	public readonly bool IsVitalDamage;

	public readonly int VitalDamageValue;

	public readonly int FinalValue;

	public readonly int ValueWithoutReduction;

	public DamageValue(IntermediateDamage source, int armorValue, int bonusArmorValue, int healthValue, bool isVital, int vitalDamageValue, int rolledValue, int reduction)
	{
		Source = source;
		ArmorValue = armorValue;
		BonusArmorValue = bonusArmorValue;
		HealthValue = healthValue;
		IsVitalDamage = isVital;
		VitalDamageValue = vitalDamageValue;
		FinalValue = armorValue + healthValue;
		RolledValue = rolledValue;
		Reduction = reduction;
		ValueWithoutReduction = FinalValue + reduction;
	}
}
