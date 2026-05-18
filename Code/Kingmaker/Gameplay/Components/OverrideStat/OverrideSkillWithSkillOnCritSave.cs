using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.OverrideStat;

[Serializable]
[ClassInfoBox("Подменяет Resistance на другой скилл только для бросков сопротивления критам (SkillCheckType.CritSave).")]
[AllowMultipleComponents]
[ComponentName("Stats/OverrideSkillWithSkillOnCritSave")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("3432f58c5a4773e4a9b470aa9ce7b847")]
public sealed class OverrideSkillWithSkillOnCritSave : UnitFactComponentDelegate, IStatModifier
{
	[Tooltip("Skill which will be used instead of Resistance during crit saves")]
	public SkillType Override;

	public bool OnlyIfHigher = true;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == SkillType.Resistance.ToStatType() && context.Rule is RuleCalculateSkillCheck { Type: SkillCheckType.CritSave })
		{
			collector.OverrideFull(Override.ToStatType(), OnlyIfHigher, base.Fact);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(SkillType.Resistance.ToStatType(), Override.ToStatType()));
	}
}
