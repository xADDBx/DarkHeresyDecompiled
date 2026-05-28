using System;
using System.Collections.Generic;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.EntitySystem.Stats.Components;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
public sealed class AddStatModifierRestrictionCalculator : RestrictionCalculator
{
	public enum StatSelectorType
	{
		Single,
		AllAttributes,
		AllSkills,
		CombatSkills,
		NonCombatSkills,
		AttributeBasedSkills
	}

	public StatSelectorType StatSelector;

	[ShowIf("IsSingleStat")]
	[ModifiableStatsFilter]
	public StatType Stat;

	[ShowIf("IsAttributeBasedSkills")]
	public AttributeType Attribute;

	[ShowIf("IsSkillMode")]
	public bool AdvancedSkillOnly;

	[InfoBox("По умолчанию на Advanced проверяется скилл Owner, но по этой галочке можно проверять скилл target")]
	[ShowIf("AdvancedSkillOnly")]
	public bool CheckTargetAdvancedSkill;

	private bool IsSingleStat => StatSelector == StatSelectorType.Single;

	private bool IsAttributeBasedSkills => StatSelector == StatSelectorType.AttributeBasedSkills;

	private bool IsSkillMode
	{
		get
		{
			if (StatSelector != StatSelectorType.AllSkills && StatSelector != StatSelectorType.CombatSkills && StatSelector != StatSelectorType.NonCombatSkills)
			{
				return StatSelector == StatSelectorType.AttributeBasedSkills;
			}
			return true;
		}
	}

	public bool IsPassed(IEvalContext evalContext, in StatContext statContext, StatType stat, MechanicEntity entity)
	{
		if (!MatchesStat(stat))
		{
			return false;
		}
		if (entity == null)
		{
			return false;
		}
		if (AdvancedSkillOnly)
		{
			MechanicEntity mechanicEntity = ((!CheckTargetAdvancedSkill) ? entity : evalContext.Target?.Entity);
			if (mechanicEntity == null || !IsAdvancedSkill(mechanicEntity, stat))
			{
				return false;
			}
		}
		return base.IsPassedInternal(entity, evalContext, null, statContext.Rule, statContext.Ability);
	}

	public new bool IsPassed(IEvalContext evalContext, in StatContext statContext)
	{
		throw new NotSupportedException("Use IsPassed(IEvalContext, StatContext, StatType)");
	}

	public new bool IsPassed(IEvalContext context, MechanicEntity currentEntity = null, TargetWrapper currentTarget = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		throw new NotSupportedException("Use IsPassed(IEvalContext, StatContext, StatType)");
	}

	public bool MatchesStat(StatType stat)
	{
		return StatSelector switch
		{
			StatSelectorType.Single => stat == Stat, 
			StatSelectorType.AllAttributes => Array.IndexOf(StatTypeHelper.Attributes, stat) >= 0, 
			StatSelectorType.AllSkills => stat.IsSkill(), 
			StatSelectorType.CombatSkills => stat.IsSkill() && stat.ToSkillType().IsCombatSkill(), 
			StatSelectorType.NonCombatSkills => stat.IsSkill() && !stat.ToSkillType().IsCombatSkill(), 
			StatSelectorType.AttributeBasedSkills => stat.IsSkill() && stat.GetBaseStat() == Attribute.ToStatType(), 
			_ => false, 
		};
	}

	public void CollectAffectedStats(ICollection<AffectedStatEntry> entries, StatType[] statDependencies)
	{
		StatType[] dependsOnStats = (AdvancedSkillOnly ? null : statDependencies);
		bool restrictionsEmpty = base.Empty && !AdvancedSkillOnly;
		switch (StatSelector)
		{
		case StatSelectorType.Single:
			entries.Add(AffectedStatEntry.Create(Stat, restrictionsEmpty, dependsOnStats));
			break;
		case StatSelectorType.AllAttributes:
		{
			StatType[] skills = StatTypeHelper.Attributes;
			foreach (StatType stat5 in skills)
			{
				entries.Add(AffectedStatEntry.Create(stat5, restrictionsEmpty, dependsOnStats));
			}
			break;
		}
		case StatSelectorType.AllSkills:
		{
			StatType[] skills = StatTypeHelper.Skills;
			foreach (StatType stat2 in skills)
			{
				entries.Add(AffectedStatEntry.Create(stat2, restrictionsEmpty, dependsOnStats));
			}
			break;
		}
		case StatSelectorType.CombatSkills:
		{
			StatType[] skills = StatTypeHelper.Skills;
			foreach (StatType stat4 in skills)
			{
				if (stat4.ToSkillType().IsCombatSkill())
				{
					entries.Add(AffectedStatEntry.Create(stat4, restrictionsEmpty, dependsOnStats));
				}
			}
			break;
		}
		case StatSelectorType.NonCombatSkills:
		{
			StatType[] skills = StatTypeHelper.Skills;
			foreach (StatType stat3 in skills)
			{
				if (!stat3.ToSkillType().IsCombatSkill())
				{
					entries.Add(AffectedStatEntry.Create(stat3, restrictionsEmpty, dependsOnStats));
				}
			}
			break;
		}
		case StatSelectorType.AttributeBasedSkills:
		{
			StatType statType = Attribute.ToStatType();
			StatType[] skills = StatTypeHelper.Skills;
			foreach (StatType stat in skills)
			{
				if (stat.GetBaseStat() == statType)
				{
					entries.Add(AffectedStatEntry.Create(stat, restrictionsEmpty, dependsOnStats));
				}
			}
			break;
		}
		}
	}

	protected override bool IsPassedInternal(MechanicEntity entity, IEvalContext context = null, TargetWrapper target = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		return base.IsPassedInternal(entity, context, target, rule, ability);
	}

	private static bool IsAdvancedSkill(MechanicEntity entity, StatType skillType)
	{
		IReadOnlyList<MechanicActor.RegisteredModifier> registeredModifiers = entity.Actor.GetRegisteredModifiers(skillType);
		if (registeredModifiers == null)
		{
			return false;
		}
		foreach (MechanicActor.RegisteredModifier item in registeredModifiers)
		{
			if (item.Component.Fact?.Blueprint is BlueprintStatAdvancement)
			{
				return true;
			}
		}
		return false;
	}
}
