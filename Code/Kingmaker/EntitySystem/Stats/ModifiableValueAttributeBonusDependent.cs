namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueAttributeBonusDependent : ModifiableValueDependent<ModifiableValueAttributeStat>
{
	public override int BaseStatBonus => base.BaseStat.Bonus;
}
