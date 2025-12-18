namespace Kingmaker.EntitySystem.Stats;

public sealed class ModifiableValueMovementPoints : ModifiableValueAttributeBonusDependent
{
	public override int BaseStatBonus => base.BaseStatBonus / 2 + base.BaseStatBonus % 2;
}
