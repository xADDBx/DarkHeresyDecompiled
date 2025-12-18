using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueHitPoints : ModifiableValueDependent
{
	private Modifier? m_BaseStatBonus;

	public override int BaseStatBonus
	{
		get
		{
			int num = base.Container.Owner.GetOptional<PartUnitProgression>()?.CharacterLevel ?? 0;
			int num2 = ((num >= 35) ? (50 + 2 * (num - 35)) : (15 + num));
			if (!base.Owner.IsCompanion)
			{
				return base.BaseValue * base.BaseStat.ModifiedValue / 100;
			}
			return num2 * base.BaseStat.ModifiedValue / 100;
		}
	}

	protected override int MinValue => 1;

	protected override void UpdateInternalModifiers()
	{
		base.UpdateInternalModifiers();
		if (base.Owner.IsCompanion)
		{
			int num = base.Container.Owner.GetOptional<PartUnitProgression>()?.CharacterLevel ?? 0;
			int num2 = 20 + num * 3;
			ref Modifier? baseStatBonus = ref m_BaseStatBonus;
			if (!baseStatBonus.HasValue || baseStatBonus.GetValueOrDefault().Value != num2)
			{
				RemoveModifier(m_BaseStatBonus);
				m_BaseStatBonus = AddInternalModifier(num2, StatType.HitPoints, ModifierDescriptor.BaseValue);
			}
		}
	}
}
