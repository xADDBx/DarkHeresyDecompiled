using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("2e982373853d4e26b7e61354b88923e0")]
public abstract class UnitDifficultyModifiersManager : UnitFactComponentDelegate, IDifficultyChangedClassHandler, ISubscriber, IUnitChangeAttackFactionsHandler, ISubscriber<IBaseUnitEntity>, IStatModifier
{
	protected override void OnActivateOrPostLoad()
	{
		OnDifficultyOrFactionChanged();
	}

	public void HandleDifficultyChanged()
	{
		OnDifficultyOrFactionChanged();
		NotifyDifficultyStatsChanged();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		if (unit == base.Owner)
		{
			OnDifficultyOrFactionChanged();
			NotifyDifficultyStatsChanged();
		}
	}

	protected virtual void OnDifficultyOrFactionChanged()
	{
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		TryApplyDifficultyModifier(collector, stat, context);
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		CollectDifficultyAffectedStats(entries);
	}

	protected virtual void TryApplyDifficultyModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
	}

	protected virtual void CollectDifficultyAffectedStats(ICollection<AffectedStatEntry> entries)
	{
	}

	protected void CollectPercentModifier(StatModifierCollector collector, StatType stat, int percentModifier)
	{
		int value = Mathf.FloorToInt((float)base.Owner.Actor.GetStatBase(stat) * ((float)percentModifier / 100f));
		collector.Modifiers.Add(ModifierType.ValAdd, value, base.Fact, null, BonusType.None, StatType.Unknown, ModifierDescriptor.Difficulty);
	}

	protected void CollectFlatModifier(StatModifierCollector collector, int flatModifier)
	{
		collector.Modifiers.Add(ModifierType.ValAdd, flatModifier, base.Fact, null, BonusType.None, StatType.Unknown, ModifierDescriptor.Difficulty);
	}

	private void NotifyDifficultyStatsChanged()
	{
		List<AffectedStatEntry> value;
		using (CollectionPool<List<AffectedStatEntry>, AffectedStatEntry>.Get(out value))
		{
			CollectDifficultyAffectedStats(value);
			ulong num = 0uL;
			foreach (AffectedStatEntry item in value)
			{
				num |= (ulong)(1L << (int)item.Stat);
			}
			if (num != 0L)
			{
				base.Owner.Actor.NotifyStatsChanged(num, "NotifyDifficultyStatsChanged");
			}
		}
	}
}
