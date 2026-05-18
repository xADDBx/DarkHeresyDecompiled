using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("WH2-11514")]
[ComponentName("Add bonus to all skills")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b18305c0363e40d4a7f0f5743a583bc5")]
[SetsContextScope(ContextEntryPointKind.BuffComponentRulebookHandler)]
public class AddContextAllSkillBonus : UnitFactComponentDelegate, IStatModifier
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public int Multiplier = 1;

	public ContextValue Value;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (Array.IndexOf(StatTypeHelper.Skills, stat) >= 0 && Restrictions.IsPassed(base.Context, in context))
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Value.Calculate(base.Context) * Multiplier, base.Fact, null, BonusType.None, StatType.Unknown, Descriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		bool isConditional = !Restrictions.Empty;
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType stat in skills)
		{
			entries.Add(new AffectedStatEntry(stat, isConditional));
		}
	}
}
