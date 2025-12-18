namespace Kingmaker.UnitLogic.Parts;

public interface IDamageablePart
{
	int Damage { get; }

	void HealDamage(int amount);
}
