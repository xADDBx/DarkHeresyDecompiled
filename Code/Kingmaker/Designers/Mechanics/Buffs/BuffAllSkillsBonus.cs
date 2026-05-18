using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete("Use AddStatModifier instead")]
[ComponentName("Facts And Buffs/BuffAllSkillsBonus (Bonus to all skills)")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("1d52ceca07db9ed4a98df9782359f75b")]
public class BuffAllSkillsBonus : UnitFactComponentDelegate, IStatModifier
{
	public ModifierDescriptor Descriptor;

	public int Value;

	public ContextValue Multiplier;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (Array.IndexOf(StatTypeHelper.Skills, stat) >= 0)
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Multiplier.Calculate(base.Context) * Value, base.Fact, null, BonusType.None, StatType.Unknown, Descriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType stat in skills)
		{
			entries.Add(new AffectedStatEntry(stat));
		}
	}
}
