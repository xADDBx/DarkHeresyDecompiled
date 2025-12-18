using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.RuleSystem.Rules.RuleDOT;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
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
[TypeId("59a93e7d46c94efab676eac75afbd08b")]
public abstract class DOTRankModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	public ContextValueModifierWithType Rank = new ContextValueModifierWithType
	{
		Enabled = true
	};

	[Header("Obsolete")]
	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier FlatValue = new ContextValueModifier();

	[Obsolete]
	[InspectorReadOnly]
	public ContextValueModifier PercentDamageModifier = new ContextValueModifier();

	protected void TryApply(RuleCalculateDOT rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			Rank.TryApply(rule.RankModifier, base.Fact, ModifierDescriptor);
			ApplyObsolete(rule);
		}
	}

	[Obsolete]
	private void ApplyObsolete(RuleCalculateDOT rule)
	{
		if (FlatValue.Enabled)
		{
			rule.RankModifier.Add(ModifierType.ValAdd, FlatValue.Value, base.Fact, ModifierDescriptor);
		}
		using (base.Context.SetScope(null, rule))
		{
			if (!PercentDamageModifier.Enabled)
			{
				return;
			}
			int num = PercentDamageModifier.Calculate(base.Context);
			if (rule.Target == base.Owner && base.Context.MaybeCaster != null)
			{
				IEnumerable<MultiplyIncomingDamageBonus> components = base.Context.MaybeCaster.Facts.GetComponents<MultiplyIncomingDamageBonus>();
				float num2 = (components.Any() ? (components.Sum((MultiplyIncomingDamageBonus p) => p.PercentIncreaseMultiplier - 1f) + 1f) : 1f);
				num = (int)((float)num * num2);
			}
			rule.RankModifier.Add(ModifierType.PctAdd, num, base.Fact, ModifierDescriptor);
		}
	}
}
