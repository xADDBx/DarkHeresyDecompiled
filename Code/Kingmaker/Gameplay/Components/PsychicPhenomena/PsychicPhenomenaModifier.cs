using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[Serializable]
[TypeId("1992183b4989453199d82a3e7930a956")]
public abstract class PsychicPhenomenaModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restriction;

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Chance = new ContextValueModifierWithType
	{
		Enabled = true
	};

	public ContextValueModifierWithType PerilsChance = new ContextValueModifierWithType();

	protected void TryApply(RulePerformPsychicPhenomena rule)
	{
		if (Restriction.IsPassed(rule.AbilityContext, base.Owner, null, rule))
		{
			Chance.TryApply(rule.ChanceModifiers, base.Fact, Descriptor);
			PerilsChance.TryApply(rule.PerilsChanceModifiers, base.Fact, Descriptor);
		}
	}
}
