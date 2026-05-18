using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[ComponentName("Stats/StatAdvancement")]
[AllowedOn(typeof(BlueprintStatAdvancement))]
[TypeId("1193f055d9ae4f5aa45e27400d7411cd")]
public class StatAdvancement : UnitFactComponentDelegate, IStatModifier
{
	private BlueprintStatAdvancement Settings => (BlueprintStatAdvancement)base.Fact.Blueprint;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == Settings.Stat)
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Settings.ValuePerRank * base.Fact.GetRank(), base.Fact, null, BonusType.None, StatType.Unknown, Settings.ModifierDescriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(Settings.Stat));
	}
}
