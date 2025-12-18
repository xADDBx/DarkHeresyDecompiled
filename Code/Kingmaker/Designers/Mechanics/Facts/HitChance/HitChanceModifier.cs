using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("349f754fc8e340fc9ba122ad784ace6c")]
public abstract class HitChanceModifier : MechanicEntityFactComponentDelegate
{
	public enum Type
	{
		All,
		Single,
		Burst,
		Aoe
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public Type AttackType;

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType HitChance = new ContextValueModifierWithType
	{
		Enabled = true,
		ModifierType = ModifierType.ValAdd
	};

	[InfoBox("Это модификаторы для положительного числа (см. CombatRoot), которое вычитается из шанса попадания")]
	public ContextValueModifierWithType PreciseAttackToCoveredTargetPenalty = new ContextValueModifierWithType();

	public bool ForceHitCover;

	public AttackHitPolicyType AttackHitPolicy;

	protected void TryApply(RuleCalculateHitChances rule)
	{
		if (IsSuitable(rule))
		{
			HitChance.TryApply(rule.Modifiers, base.Fact, Descriptor);
			PreciseAttackToCoveredTargetPenalty.TryApply(rule.PreciseAttackToCoveredTargetPenaltyModifiers, base.Fact, Descriptor);
			if (ForceHitCover)
			{
				rule.ForceHitCover.Add(base.Fact);
			}
			switch (AttackHitPolicy)
			{
			case AttackHitPolicyType.AutoHit:
				rule.AutoHit.Add(base.Fact);
				break;
			case AttackHitPolicyType.AutoMiss:
				rule.AutoMiss.Add(base.Fact);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AttackHitPolicyType.Default:
				break;
			}
		}
	}

	private bool IsSuitable(RuleCalculateHitChances rule)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, rule))
		{
			return false;
		}
		switch (AttackType)
		{
		case Type.Single:
			if (!rule.Ability.IsSingleTarget)
			{
				return false;
			}
			break;
		case Type.Burst:
			if (!rule.Ability.IsBurst)
			{
				return false;
			}
			break;
		case Type.Aoe:
			if (!rule.Ability.IsAoe)
			{
				return false;
			}
			break;
		}
		return true;
	}
}
