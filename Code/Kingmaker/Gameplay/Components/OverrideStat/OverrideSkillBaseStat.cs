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
[ClassInfoBox("Подменяет базовый атрибут скилла.")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d503099978ed4daeb041048174fe66cb")]
public sealed class OverrideSkillBaseStat : UnitFactComponentDelegate, IStatModifier
{
	public SkillType Target;

	public AttributeType Override;

	public bool OnlyIfHigher = true;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == Target.ToStatType())
		{
			collector.OverrideBase(Override.ToStatType(), OnlyIfHigher, base.Fact);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(Target.ToStatType(), Override.ToStatType()));
	}
}
