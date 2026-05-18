using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.Gameplay.Stats;

public static class StatBuiltInModifierConfig
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void RegisterAll()
	{
		BuiltInStatModifiers.Register(StatType.ArmorDamageReduction, ApplyArmorDamageReductionRules);
	}

	private static void ApplyArmorDamageReductionRules(StatModifierCollector collector, StatType stat, StatContext context)
	{
		DamageType? damageType = context.DamageType;
		if (damageType.HasValue)
		{
			DamageType valueOrDefault = damageType.GetValueOrDefault();
			if (valueOrDefault.GetInfo().IgnoreArmor || valueOrDefault == DamageType.Toxic)
			{
				collector.Modifiers.Add(ModifierType.PctMul_Extra, 0, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.Weapon);
				return;
			}
		}
		BlueprintBodyPart bodyPart = context.BodyPart;
		if (bodyPart != null && bodyPart.IgnoreArmorDamageReduction)
		{
			collector.Modifiers.Add(ModifierType.PctMul_Extra, 0, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.Weakpoint);
		}
	}
}
