using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("68a816b53549452587acc6a2ba5a94fc")]
public abstract class HealModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	protected void TryApply(RuleCalculateHeal rule)
	{
		MechanicsContext context = rule.Context;
		if (context != null && Restrictions.IsPassed(context, base.Owner, null, rule))
		{
			Modifier.TryApply(rule.Modifiers, base.Fact, Descriptor);
		}
	}
}
