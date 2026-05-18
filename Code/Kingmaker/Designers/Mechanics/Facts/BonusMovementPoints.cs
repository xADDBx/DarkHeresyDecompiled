using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("Use AddStatModifier with stat MovementPoints instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Movement/BonusMovementPoints")]
[TypeId("6d4d8e393e06464abf749d2b80d67adc")]
public class BonusMovementPoints : UnitFactComponentDelegate, IStatModifier
{
	public int Bonus;

	public float Modifier = 1f;

	public ContextValue Value;

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == StatType.MovementPoints)
		{
			collector.Modifiers.Add(ModifierType.ValAdd, Mathf.RoundToInt((float)(Bonus + Value.Calculate(base.Context)) * Modifier), base.Fact);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(StatType.MovementPoints));
	}
}
