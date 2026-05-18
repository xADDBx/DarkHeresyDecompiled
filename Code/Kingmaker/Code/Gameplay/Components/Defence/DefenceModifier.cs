using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[Obsolete("Use AddStatModifier instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("8ace7d5a46f843d197a09a644992baed")]
[SetsContextScope(ContextEntryPointKind.BuffComponentRulebookHandler)]
public abstract class DefenceModifier : MechanicEntityFactComponentDelegate, IStatModifier
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[InfoBox("Тип зависимостей модификатора, должен соответствовать рестрикшенам. None - рестрикшенов нет, OwnerStat - рестрикшен на стат юнита получающего модификатор (+20 силы пока ХП < 50%), External - любые другие рестрикшены.")]
	public StatRestrictionDependency DependencyType;

	[InfoBox("От каких статов юнита получающего модификатор зависит этот модификатор (+20 силы пока ХП < 50%). Нужен для DependencyType = OwnerStat. Должен соответствовать рестрикшенам.")]
	[ShowIf("ShowStatDependencies")]
	public StatType[] StatDependencies = new StatType[0];

	public ContextValue Defence = new ContextValue();

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

	protected abstract StatModifierScope Scope { get; }

	StatModifierScope IStatModifier.Scope => Scope;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == StatType.Defence && Restrictions.IsPassed(base.Context, in context))
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Defence.Calculate(base.Context), base.Fact);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		StatType[] dependsOnStats = ((DependencyType == StatRestrictionDependency.OwnerStat) ? StatDependencies : null);
		entries.Add(AffectedStatEntry.Create(StatType.Defence, Restrictions.Empty, dependsOnStats));
	}
}
