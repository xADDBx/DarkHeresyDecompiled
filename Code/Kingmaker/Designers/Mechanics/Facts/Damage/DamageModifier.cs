using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[TypeId("b05381e4f203a76418913b0f6b5323f8")]
public abstract class DamageModifier : MechanicEntityFactComponentDelegate
{
	[Flags]
	public enum FilterFlags
	{
		Normal = 1,
		DOT = 2,
		Direct = 4
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	[EnumFlagsAsButtons(ColumnCount = 3)]
	public FilterFlags Filter = FilterFlags.Normal;

	public ContextValueModifierWithType Damage = new ContextValueModifierWithType();

	public ContextValueModifierWithType VitalDamage = new ContextValueModifierWithType();

	public ContextValueModifierWithType ArmorDamage = new ContextValueModifierWithType();

	public ContextValueModifierWithType HealthDamage = new ContextValueModifierWithType();

	public ContextValueModifierWithType CritsCount = new ContextValueModifierWithType();

	public ContextValueModifierWithType DamageReduction = new ContextValueModifierWithType();

	public DamageStrategy ApplyDamageStrategy;

	public VitalDamageStrategy ApplyVitalDamageStrategy;

	[Header("Obsolete")]
	[Obsolete]
	[InspectorReadOnly]
	public bool ModifyEvenDirectDamage;

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier MinimumDamage = new ContextValueModifier();

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier MaximumDamage = new ContextValueModifier();

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier PercentDamageModifier = new ContextValueModifier();

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier UnmodifiableFlatDamageModifier = new ContextValueModifier();

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier UnmodifiablePercentDamageModifier = new ContextValueModifier();

	protected void TryApply(RuleCalculateDamage rule)
	{
		if (IsSuitable(rule) && Restrictions.IsPassed(base.Context, null, null, rule))
		{
			Damage.TryApply(rule.Modifiers, base.Fact, ModifierDescriptor);
			VitalDamage.TryApply(rule.VitalModifiers, base.Fact, ModifierDescriptor);
			ArmorDamage.TryApply(rule.ArmorDamageModifiers, base.Fact, ModifierDescriptor);
			HealthDamage.TryApply(rule.HealthDamageModifiers, base.Fact, ModifierDescriptor);
			CritsCount.TryApply(rule.CritsCountModifiers, base.Fact, ModifierDescriptor);
			DamageReduction.TryApply(rule.DamageReduction, base.Fact, ModifierDescriptor);
			if (ApplyDamageStrategy != 0)
			{
				rule.PushApplyStrategy(ApplyDamageStrategy, base.Runtime, ModifierDescriptor);
			}
			if (ApplyVitalDamageStrategy != 0)
			{
				rule.PushApplyStrategy(ApplyVitalDamageStrategy, base.Runtime, ModifierDescriptor);
			}
			ApplyObsolete(rule);
		}
	}

	private bool IsSuitable(RuleCalculateDamage rule)
	{
		bool flag = rule.Reason.Fact?.Blueprint is BlueprintBuff blueprintBuff && blueprintBuff.AbilityGroups.Contains(ConfigRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup);
		if ((Filter & FilterFlags.DOT) == 0 && flag)
		{
			return false;
		}
		bool flag2 = rule.DamageType == DamageType.Direct;
		if ((Filter & FilterFlags.Direct) == 0 && flag2 && !ModifyEvenDirectDamage)
		{
			return false;
		}
		return (Filter & FilterFlags.Normal) != 0 || flag || flag2;
	}

	[Obsolete]
	private void ApplyObsolete(RuleCalculateDamage rule)
	{
		if (PercentDamageModifier.Enabled)
		{
			int num = PercentDamageModifier.Calculate(base.Context);
			if (rule.MaybeTarget == base.Owner && base.Context.MaybeCaster != null)
			{
				IEnumerable<MultiplyIncomingDamageBonus> components = base.Context.MaybeCaster.Facts.GetComponents<MultiplyIncomingDamageBonus>();
				float num2 = (components.Any() ? (components.Sum((MultiplyIncomingDamageBonus p) => p.PercentIncreaseMultiplier - 1f) + 1f) : 1f);
				num = (int)((float)num * num2);
			}
			rule.Modifiers.Add(ModifierType.PctAdd, num, base.Fact, ModifierDescriptor);
		}
		if (MinimumDamage.Enabled)
		{
			rule.MinValueModifiers.Add(MinimumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (MaximumDamage.Enabled)
		{
			rule.MaxValueModifiers.Add(MaximumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (UnmodifiableFlatDamageModifier.Enabled)
		{
			rule.Modifiers.Add(ModifierType.ValAdd_Extra, UnmodifiableFlatDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (UnmodifiablePercentDamageModifier.Enabled)
		{
			rule.Modifiers.Add(ModifierType.PctMul_Extra, UnmodifiablePercentDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
	}
}
