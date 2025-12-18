using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.UnitLogic.Abilities;

public struct UIHealPredictionData
{
	public int Bonus;

	public int MinHeal;

	public int MaxHeal;

	public DamageStrategy HealStrategy;

	public bool Equals(UIHealPredictionData other)
	{
		if (object.Equals(Bonus, other.Bonus) && object.Equals(MinHeal, other.MinHeal) && object.Equals(MaxHeal, other.MaxHeal))
		{
			return object.Equals(HealStrategy, other.HealStrategy);
		}
		return false;
	}
}
