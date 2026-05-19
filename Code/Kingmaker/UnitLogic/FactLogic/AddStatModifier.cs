using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("Stats/AddStatModifier")]
[TypeId("f08844ce14d498a45a9fc64582489a2a")]
[SetsContextScope(ContextEntryPointKind.BuffComponentRulebookHandler)]
public sealed class AddStatModifier : UnitFactComponentDelegate, IStatModifier, ISerializationCallbackReceiver
{
	public AddStatModifierRestrictionCalculator Restrictions = new AddStatModifierRestrictionCalculator();

	[InfoBox("Тип зависимостей модификатора, должен соответствовать рестрикшенам. None - рестрикшенов нет, OwnerStat - рестрикшен на стат юнита получающего модификатор, External - любые другие рестрикшены.")]
	public StatRestrictionDependency DependencyType;

	[InfoBox("От каких статов юнита получающего модификатор зависит этот модификатор. Нужен для DependencyType = OwnerStat. Должен соответствовать рестрикшенам.")]
	[ShowIf("ShowStatDependencies")]
	public StatType[] StatDependencies = new StatType[0];

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Value = new ContextValueModifierWithType
	{
		Enabled = true
	};

	[InfoBox("Где модификатор применяется при расчёте стата:\nOwner — стат носителя факта (по умолчанию).\nAgainst — стат оппонента, когда носитель факта — атакующая сторона.\nGlobal — стат любого юнита (через GlobalStatModifierRegistry).")]
	public StatModifierScope Scope;

	[Obsolete("Serialization migration only. Moved to Restrictions.")]
	[HideInInspector]
	public AddStatModifierRestrictionCalculator.StatSelectorType StatSelector;

	[Obsolete("Serialization migration only. Moved to Restrictions.")]
	[HideInInspector]
	public StatType Stat;

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

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (Value.Enabled && Restrictions.IsPassed(base.Context, in context, stat))
		{
			int value = Value.Calculate(base.Context);
			collector.Modifiers.Add(Value.ModifierType, value, base.Fact, null, BonusType.None, StatType.Unknown, Descriptor);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		if (Value.Enabled)
		{
			Restrictions.CollectAffectedStats(entries, (DependencyType == StatRestrictionDependency.OwnerStat) ? StatDependencies : null);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (Restrictions == null)
		{
			Restrictions = new AddStatModifierRestrictionCalculator();
		}
		if (Restrictions.StatSelector == AddStatModifierRestrictionCalculator.StatSelectorType.Single && Restrictions.Stat == StatType.Unknown)
		{
			Restrictions.StatSelector = StatSelector;
			Restrictions.Stat = Stat;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}
}
