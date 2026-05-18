using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components.OverrideStat;

[Serializable]
[ClassInfoBox("Подменяет один атрибут другим.")]
[AllowMultipleComponents]
[ComponentName("Stats/OverrideStat")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d553b17d00fd44b085c1eb5c79c8e94f")]
public sealed class OverrideStat : UnitFactComponentDelegate, IStatModifier
{
	public AttributeType Target;

	public AttributeType Override;

	public bool OnlyIfHigher = true;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == Target.ToStatType())
		{
			collector.OverrideFull(Override.ToStatType(), OnlyIfHigher, base.Fact);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(Target.ToStatType(), Override.ToStatType()));
	}
}
