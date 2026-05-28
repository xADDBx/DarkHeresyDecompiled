using System;
using System.Collections.Generic;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("1c8015cd817446dfafe7f9b1421512e6")]
[ClassInfoBox("Модифицирует шанс атаки, защиты и скилл чеков")]
[SetsContextScope(ContextEntryPointKind.BuffComponentRulebookHandler)]
public abstract class RollDifficultyModifier : MechanicEntityFactComponentDelegate, IStatModifier
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	[InfoBox("Тип зависимостей модификатора, должен соответствовать рестрикшенам. None - рестрикшенов нет, OwnerStat - рестрикшен на стат юнита получающего модификатор, External - любые другие рестрикшены.")]
	public StatRestrictionDependency DependencyType;

	[InfoBox("От каких статов юнита получающего модификатор зависит этот модификатор. Нужен для DependencyType = OwnerStat.")]
	[ShowIf("ShowStatDependencies")]
	public StatType[] StatDependencies = new StatType[0];

	protected abstract StatModifierScope Scope { get; }

	StatModifierScope IStatModifier.Scope => Scope;

	private bool ShowStatDependencies
	{
		get
		{
			if (DependencyType != StatRestrictionDependency.OwnerStat)
			{
				StatType[] statDependencies = StatDependencies;
				if (statDependencies != null)
				{
					return statDependencies.Length > 0;
				}
				return false;
			}
			return true;
		}
	}

	protected void TryApply(RulebookEvent rule)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, rule))
		{
			return;
		}
		if (!(rule is RuleCalculateHitChances ruleCalculateHitChances))
		{
			if (!(rule is RuleCalculateSkillCheck ruleCalculateSkillCheck))
			{
				throw new ArgumentOutOfRangeException();
			}
			Modifier.TryApply(ruleCalculateSkillCheck.DifficultyModifiers, base.Fact, Descriptor);
		}
		else
		{
			Modifier.TryApply(ruleCalculateHitChances.Modifiers, base.Fact, Descriptor);
		}
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == StatType.Defence && (context.Rule != null || !(context.Ability == null) || DependencyType == StatRestrictionDependency.OwnerStat || Restrictions.Empty))
		{
			MechanicEntity currentEntity = ((Scope == StatModifierScope.Against) ? context.Against : context.Owner)?.Entity;
			if (Restrictions.IsPassed(base.Context, currentEntity, null, context.Rule, context.Ability))
			{
				Modifier.TryApply(collector.Modifiers, base.Fact, Descriptor);
			}
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		if (Modifier.Enabled)
		{
			StatType[] dependsOnStats = ((DependencyType == StatRestrictionDependency.OwnerStat) ? StatDependencies : null);
			entries.Add(AffectedStatEntry.Create(StatType.Defence, Restrictions.Empty, dependsOnStats));
		}
	}
}
