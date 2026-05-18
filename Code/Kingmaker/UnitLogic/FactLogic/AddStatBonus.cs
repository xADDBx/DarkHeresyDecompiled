using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.EntitySystem.Stats.Components;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("WH2-11514")]
[ComponentName("Add stat bonus")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("a2844c135c0324e439072bd3cc2f9260")]
public class AddStatBonus : UnitFactComponentDelegate, IStatModifier
{
	public ModifierDescriptor Descriptor;

	[ModifiableStatsFilter]
	public StatType Stat;

	public int Value;

	public static int TryApplyArcanistPowerfulChange(MechanicsContext context, StatType stat, int value)
	{
		return value;
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == Stat)
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Value * base.Fact.GetRank(), base.Fact, null, BonusType.None, StatType.Unknown, Descriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(Stat));
	}
}
