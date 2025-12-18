using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete("Use DamageModifier instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("af60bec0ca8d4126a3841608d620e8ec")]
public abstract class VitalDamageModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType VitalDamage;

	protected void TryApply(RuleCalculateDamage evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			evt.VitalModifiers.Add(VitalDamage.ModifierType, VitalDamage.Calculate(base.Context), base.Fact);
		}
	}
}
