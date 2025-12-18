using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
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
public class BonusMovementPoints : UnitFactComponentDelegate
{
	public int Bonus;

	public float Modifier = 1f;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		int value = Mathf.RoundToInt((float)(Bonus + Value.Calculate(base.Context)) * Modifier);
		base.Owner.GetStatOptional(StatType.MovementPoints)?.AddModifier(value, base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetStatOptional(StatType.MovementPoints)?.RemoveModifiersFrom(base.Runtime);
	}
}
