using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.OverrideStat;

[Serializable]
[ClassInfoBox("Подменяет значение скилла другим скиллом.")]
[AllowMultipleComponents]
[ComponentName("Stats/OverrideSkillWithSkill")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0edb73869b529a24f8d4cc89f8afe250")]
public sealed class OverrideSkillWithSkill : UnitFactComponentDelegate, IStatModifier
{
	[Tooltip("Skills which will be overriden")]
	public List<SkillType> Targets;

	[Tooltip("Skill which will be used instead of Targets")]
	public SkillType Override;

	public bool OnlyIfHigher = true;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		foreach (SkillType target in Targets)
		{
			if (stat == target.ToStatType())
			{
				collector.OverrideFull(Override.ToStatType(), OnlyIfHigher, base.Fact);
				break;
			}
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		foreach (SkillType target in Targets)
		{
			entries.Add(new AffectedStatEntry(target.ToStatType(), Override.ToStatType()));
		}
	}
}
