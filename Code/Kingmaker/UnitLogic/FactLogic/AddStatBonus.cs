using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("WH2-11514")]
[ComponentName("Add stat bonus")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("a2844c135c0324e439072bd3cc2f9260")]
public class AddStatBonus : UnitFactComponentDelegate
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public int Value;

	protected override void OnActivateOrPostLoad()
	{
		ModifiableValue stat = base.Owner.Stats.GetStat(Stat);
		int value = Value * base.Fact.GetRank();
		value = TryApplyArcanistPowerfulChange(base.Context, Stat, value);
		stat?.AddModifier(value, base.Runtime, Descriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat).RemoveModifiersFrom(base.Runtime);
	}

	public static int TryApplyArcanistPowerfulChange(MechanicsContext context, StatType stat, int value)
	{
		return value;
	}
}
