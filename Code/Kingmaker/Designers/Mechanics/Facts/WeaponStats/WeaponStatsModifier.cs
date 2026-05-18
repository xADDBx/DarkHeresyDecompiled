using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("06c886b0298c417a982b70b9df35ba6d")]
public abstract class WeaponStatsModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Damage = new ContextValueModifierWithType();

	public ContextValueModifier DamageMin = new ContextValueModifier();

	public ContextValueModifier DamageMax = new ContextValueModifier();

	public ContextValueModifierWithType AdditionalHealthDamage = new ContextValueModifierWithType();

	public ContextValueModifierWithType AdditionalArmorDamage = new ContextValueModifierWithType();

	public ContextValueModifierWithType AdditionalHitChance = new ContextValueModifierWithType();

	public ContextValueModifierWithType MaxDistance = new ContextValueModifierWithType();

	public ContextValueModifierWithType RateOfFire = new ContextValueModifierWithType();

	public ContextValueModifierWithType Recoil = new ContextValueModifierWithType();

	protected void TryApply(RuleCalculateStatsWeapon rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			Apply(rule);
		}
	}

	protected void Apply(RuleCalculateStatsWeapon rule)
	{
		Damage.TryApply(rule.BaseDamage.Modifiers, base.Fact, Descriptor);
		DamageMin.TryApply(rule.BaseDamage.MinValueModifiers, base.Fact, Descriptor);
		DamageMax.TryApply(rule.BaseDamage.MaxValueModifiers, base.Fact, Descriptor);
		AdditionalHealthDamage.TryApply(rule.BaseDamage.HealthDamageModifiers, base.Fact, Descriptor);
		AdditionalArmorDamage.TryApply(rule.BaseDamage.ArmorDamageModifiers, base.Fact, Descriptor);
		AdditionalHitChance.TryApply(rule.AdditionalHitChanceModifiers, base.Fact, Descriptor);
		MaxDistance.TryApply(rule.MaxDistanceModifiers, base.Fact, Descriptor);
		RateOfFire.TryApply(rule.RateOfFireModifiers, base.Fact, Descriptor);
		Recoil.TryApply(rule.RecoilModifiers, base.Fact, Descriptor);
	}
}
