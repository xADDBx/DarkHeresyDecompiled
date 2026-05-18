using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace Kingmaker.Gameplay.Stats;

public static class StatDependencyConfig
{
	private const int StatBonusDivisor = 10;

	private const int CompanionBaseWounds = 14;

	private const int ToughnessPercentDivisor = 100;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void RegisterAll()
	{
		RegisterSkills();
		RegisterDefence();
		RegisterInitiative();
		RegisterResolve();
		RegisterMovementPoints();
		RegisterHitPoints();
		RegisterItemArmorStats();
		RegisterReadonlyStatNotifications();
		StatDependencyRegistry.BuildReverseDependencies();
	}

	private static void RegisterSkills()
	{
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType statType in skills)
		{
			if (StatTypeHelper.BaseStats.TryGetValue(statType, out var value))
			{
				StatDependencyRegistry.Register(statType, value, ApplyAttributeFullValue);
			}
		}
	}

	private static void RegisterDefence()
	{
		StatDependencyRegistry.Register(StatType.Defence, StatType.Agility, ApplyAttributeBonusValue);
	}

	private static void RegisterInitiative()
	{
		StatDependencyRegistry.Register(StatType.Initiative, StatType.Agility, ApplyAttributeBonusValue);
	}

	private static void RegisterResolve()
	{
		StatDependencyRegistry.Register(StatType.Resolve, StatType.Fellowship, ApplyAttributeBonusValue);
	}

	private static void RegisterMovementPoints()
	{
		StatDependencyRegistry.Register(StatType.MovementPoints, StatType.Agility, ApplyMovementPointsBonus);
	}

	private static void RegisterHitPoints()
	{
		StatDependencyRegistry.Register(StatType.MaxHitPoints, StatType.Toughness, ApplyHitPointsBonus);
	}

	private static void ApplyAttributeFullValue(StatModifierCollector c, int baseStatValue, int rawBase, MechanicEntity entity, StatType baseStat)
	{
		c.AddBaseStatBonus(baseStatValue, baseStat);
	}

	private static void ApplyAttributeBonusValue(StatModifierCollector c, int baseStatValue, int rawBase, MechanicEntity entity, StatType baseStat)
	{
		c.AddBaseStatBonus(baseStatValue / 10, baseStat);
	}

	private static void ApplyMovementPointsBonus(StatModifierCollector c, int baseStatValue, int rawBase, MechanicEntity entity, StatType baseStat)
	{
		int num = baseStatValue / 10;
		c.AddBaseStatBonus((num + 1) / 2, baseStat);
	}

	private static void RegisterItemArmorStats()
	{
		StatDependencyRegistry.Register(StatType.ArmorDamageReduction, StatType.ItemArmorDamageReduction, ApplyAttributeFullValue);
		StatDependencyRegistry.Register(StatType.MaxArmorDurability, StatType.ItemArmorAmount, ApplyAttributeFullValue);
	}

	private static void RegisterReadonlyStatNotifications()
	{
		StatDependencyRegistry.Register(StatType.CurrentHitPoints, StatType.MaxHitPoints, NoOpApply);
		StatDependencyRegistry.Register(StatType.CurrentArmorDurability, StatType.MaxArmorDurability, NoOpApply);
	}

	private static void NoOpApply(StatModifierCollector c, int baseStatValue, int rawBase, MechanicEntity entity, StatType baseStat)
	{
	}

	private static void ApplyHitPointsBonus(StatModifierCollector c, int toughnessValue, int rawBase, MechanicEntity entity, StatType baseStat)
	{
		if (entity.IsCompanion)
		{
			int num = entity.GetOptional<PartUnitProgression>()?.CharacterLevel ?? 0;
			int num2 = 14 + num * (num + 1) / 2;
			c.Modifiers.Add(ModifierType.ValAdd, num2, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
			c.AddBaseStatBonus(num2 * toughnessValue / 100, baseStat);
		}
		else
		{
			c.AddBaseStatBonus(rawBase * toughnessValue / 100, baseStat);
		}
	}
}
