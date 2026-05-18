using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.EntitySystem.Stats.Components;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("Stats/StatReductionBuff")]
[TypeId("a64c5f5bc18aa7e439187c400cbe5a38")]
public class StatReductionBuff : UnitBuffComponentDelegate, IStatModifier
{
	public ModifierDescriptor Descriptor;

	[ModifiableStatsFilter]
	public StatType Stat;

	public bool IsPercentReduction;

	[HideIf("IsPercentReduction")]
	public ContextValue ReductionValue;

	[HideInInspector]
	public DiceFormula Value;

	[HideInInspector]
	public int Bonus;

	[ShowIf("IsPercentReduction")]
	[Tooltip("The percentage by which the stat will be reduced")]
	[Range(0f, 100f)]
	public int ReductionPercent;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == Stat)
		{
			int num;
			if (IsPercentReduction)
			{
				num = base.Owner.Actor.GetStatBase(Stat) * ReductionPercent / 100;
			}
			else
			{
				ContextValue reductionValue = ReductionValue;
				num = ((reductionValue == null || reductionValue.IsZero) ? ((Value.MinValue(Bonus) + Value.MaxValue(Bonus)) / 2) : ReductionValue.Calculate(base.Context));
			}
			collector.Modifiers.Add(ModifierType.ValAdd, -num, base.Fact, null, BonusType.None, StatType.Unknown, Descriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(Stat));
	}
}
