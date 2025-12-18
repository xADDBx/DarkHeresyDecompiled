using System;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Serializable]
public class DamageTypeSettings
{
	public DamageType Type;

	public IntermediateDamage CreateDamage(DiceFormula dice, int bonus)
	{
		return Type.CreateDamage(dice, bonus);
	}

	public IntermediateDamage CreateDamage(int min, int max)
	{
		return Type.CreateDamage(min, max);
	}

	public IntermediateDamage CreateDamage(int value)
	{
		return Type.CreateDamage(value, value);
	}

	public override string ToString()
	{
		return $"Damage[{Type}]";
	}
}
