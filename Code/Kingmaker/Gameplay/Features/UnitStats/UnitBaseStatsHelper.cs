using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.EntitySystem.Stats.Components;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Unit.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.UnitStats;

public static class UnitBaseStatsHelper
{
	private static BlueprintUnitStatsRoot Config => ConfigRoot.Instance.UnitStatsRoot;

	public static ImmutableDictionary<StatType, StatBaseValue> CalculateStats(BlueprintUnit blueprint, NPCDifficultyOption npcDifficulty, int cr, Func<AttributeType, AttributeCategoryType> attributeCategoryType)
	{
		Dictionary<StatType, StatBaseValue> value;
		using (CollectionPool<Dictionary<StatType, StatBaseValue>, KeyValuePair<StatType, StatBaseValue>>.Get(out value))
		{
			StatType[] allStats = StatTypeHelper.AllStats;
			foreach (StatType statType in allStats)
			{
				if (statType.IsAttribute())
				{
					AttributeType attributeType = statType.ToAttributeType();
					value.Add(statType, GetAttributeValue(attributeType, blueprint, cr, attributeCategoryType(attributeType)));
					continue;
				}
				if (statType.IsSkill())
				{
					SkillType skill = statType.ToSkillType();
					value.Add(statType, GetSkillValue(skill, blueprint, cr, attributeCategoryType(skill.GetBaseAttribute())));
					continue;
				}
				StatBaseValue value2 = statType switch
				{
					StatType.HitPoints => GetHitPointsValue(blueprint, npcDifficulty, cr), 
					StatType.ArmorDurability => GetArmorDurabilityValue(blueprint, npcDifficulty, cr), 
					StatType.Defence => blueprint.Defence, 
					StatType.ArmorDamageReduction => blueprint.ArmorDamageReduction, 
					StatType.MovementPoints => blueprint.WarhammerInitialAPBlue, 
					StatType.ActionPoints => blueprint.WarhammerInitialAPYellow, 
					StatType.CohesionRange => ConfigRoot.Instance.CombatRoot.DefaultCohesionRange, 
					_ => 0, 
				};
				value.Add(statType, value2);
			}
			return value.ToImmutableDictionary();
		}
	}

	public static ImmutableDictionary<AttributeType, AttributeCategory> CalculateAttributeCategories(BlueprintUnit blueprint)
	{
		if (blueprint.IsCompanion)
		{
			return ImmutableDictionary<AttributeType, AttributeCategory>.Empty;
		}
		Dictionary<AttributeType, AttributeCategory> value;
		using (CollectionPool<Dictionary<AttributeType, AttributeCategory>, KeyValuePair<AttributeType, AttributeCategory>>.Get(out value))
		{
			BlueprintUnitStatsRoot.AttributeCategoryAdvance[] defaultAttributeAdvances = Config.DefaultAttributeAdvances;
			foreach (BlueprintUnitStatsRoot.AttributeCategoryAdvance advance in defaultAttributeAdvances)
			{
				TryApplyAdvance(blueprint, value, advance, AttributeCategoryAdvanceType.Default);
			}
			defaultAttributeAdvances = Config.GetDifficultyTypeAdvances(blueprint.DifficultyType).Advances;
			foreach (BlueprintUnitStatsRoot.AttributeCategoryAdvance advance2 in defaultAttributeAdvances)
			{
				TryApplyAdvance(blueprint, value, advance2, AttributeCategoryAdvanceType.DifficultyType);
			}
			defaultAttributeAdvances = Config.GetUnitSubtypeAdvances(blueprint.Subtype).Advances;
			foreach (BlueprintUnitStatsRoot.AttributeCategoryAdvance advance3 in defaultAttributeAdvances)
			{
				TryApplyAdvance(blueprint, value, advance3, AttributeCategoryAdvanceType.Subtype);
			}
			UnitStatModifiers.AttributeCategoryIncrease[] array = blueprint.Army?.StatModifiers.Attributes;
			if (array != null)
			{
				UnitStatModifiers.AttributeCategoryIncrease[] array2 = array;
				foreach (UnitStatModifiers.AttributeCategoryIncrease attributeCategoryIncrease in array2)
				{
					ApplyAdvance(value, attributeCategoryIncrease.Attribute, attributeCategoryIncrease.CategoryIncrease, AttributeCategoryAdvanceType.ArmyType);
				}
			}
			UnitStatModifiers.AttributeCategoryIncrease[] attributes = blueprint.StatModifiers.Attributes;
			if (attributes != null)
			{
				UnitStatModifiers.AttributeCategoryIncrease[] array2 = attributes;
				foreach (UnitStatModifiers.AttributeCategoryIncrease attributeCategoryIncrease2 in array2)
				{
					ApplyAdvance(value, attributeCategoryIncrease2.Attribute, attributeCategoryIncrease2.CategoryIncrease, AttributeCategoryAdvanceType.Blueprint);
				}
			}
			UnitStatModifiers.AttributeCategoryIncrease[] array3 = blueprint.Template.Get()?.StatModifiers.Attributes;
			if (array3 != null)
			{
				UnitStatModifiers.AttributeCategoryIncrease[] array2 = array3;
				foreach (UnitStatModifiers.AttributeCategoryIncrease attributeCategoryIncrease3 in array2)
				{
					ApplyAdvance(value, attributeCategoryIncrease3.Attribute, attributeCategoryIncrease3.CategoryIncrease, AttributeCategoryAdvanceType.UnitTemplate);
				}
			}
			return value.ToImmutableDictionary();
		}
	}

	private static int GetAttributeValue(AttributeType attribute, BlueprintUnit blueprint, int cr, AttributeCategoryType category)
	{
		int? num = blueprint.GetComponent<UnitAttributesComponent>()?.GetAttribute(attribute);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			return Math.Max(0, valueOrDefault);
		}
		if (blueprint.IsCompanion)
		{
			return 30;
		}
		float difficultyTypeAttributeFactor = Config.GetDifficultyTypeAttributeFactor(blueprint.DifficultyType);
		return RoundToFive(Mathf.RoundToInt((float)GetUnmodifiedAttributeValue(category, cr) * difficultyTypeAttributeFactor));
	}

	private static int GetSkillValue(SkillType skill, BlueprintUnit blueprint, int cr, AttributeCategoryType category)
	{
		int? num = blueprint.GetComponent<UnitSkillsComponent>()?.GetAttribute(skill);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			return Math.Max(0, valueOrDefault);
		}
		if (blueprint.IsCompanion)
		{
			return 0;
		}
		return category switch
		{
			AttributeCategoryType.Dump => 0, 
			AttributeCategoryType.Tertiary => 0, 
			AttributeCategoryType.Secondary => 10 * Mathf.FloorToInt((float)cr / 10f), 
			AttributeCategoryType.Primary => 10 * Mathf.FloorToInt((float)cr / 8f), 
			AttributeCategoryType.PrimaryPlus => 10 * Mathf.FloorToInt((float)cr / 8f), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static StatBaseValue GetHitPointsValue(BlueprintUnit blueprint, NPCDifficultyOption npcDifficulty, int cr)
	{
		UnitHitPointsComponent component = blueprint.GetComponent<UnitHitPointsComponent>();
		if (component != null)
		{
			int value = component.Value;
			bool forced = component.Forced;
			return new StatBaseValue(Math.Max(1, value), enabled: true, forced);
		}
		if (blueprint.IsCompanion)
		{
			return 0;
		}
		float unmodifiedHitPoints = GetUnmodifiedHitPoints(blueprint, npcDifficulty, cr);
		float num = (blueprint.IsTough() ? (1f + (float)Config.ToughUnitHitPointsModifier / 100f) : (blueprint.IsFragile() ? (1f + (float)Config.FragileUnitHitPointsModifier / 100f) : 1f));
		int? num2 = blueprint.Army?.StatModifiers.HitPoints;
		float num3;
		if (num2.HasValue)
		{
			int valueOrDefault = num2.GetValueOrDefault();
			num3 = 1f + (float)valueOrDefault / 100f;
		}
		else
		{
			num3 = 1f;
		}
		float num4 = num3;
		float num5 = 1f + (float)blueprint.StatModifiers.HitPoints / 100f;
		float num6 = num * num4 * num5;
		return (int)Math.Round(unmodifiedHitPoints * num6);
	}

	private static int GetArmorDurabilityValue(BlueprintUnit blueprint, NPCDifficultyOption npcDifficulty, int cr)
	{
		int? num = blueprint.GetComponent<UnitArmorDurabilityComponent>()?.Value;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			return Math.Max(0, valueOrDefault);
		}
		if (blueprint.UseArmorOfEquipment)
		{
			return 0;
		}
		float unmodifiedHitPoints = GetUnmodifiedHitPoints(blueprint, npcDifficulty, cr);
		float num2 = (blueprint.IsFragile() ? (1f + (float)Config.FragileUnitArmorFromHitPointsModifier / 100f) : (blueprint.IsTank() ? (1f + (float)Config.TankUnitArmorFromHitPointsModifier / 100f) : (1f + (float)Config.DefaultUnitArmorFromHitPointsModifier / 100f)));
		num = blueprint.Army?.StatModifiers.ArmorDurability;
		float num3;
		if (num.HasValue)
		{
			int valueOrDefault2 = num.GetValueOrDefault();
			num3 = 1f + (float)valueOrDefault2 / 100f;
		}
		else
		{
			num3 = 1f;
		}
		float num4 = num3;
		float num5 = 1f + (float)blueprint.StatModifiers.ArmorDurability / 100f;
		float num6 = num2 * num4 * num5;
		return (int)Math.Round(unmodifiedHitPoints * num6);
	}

	private static int GetUnmodifiedAttributeValue(AttributeCategoryType category, int cr)
	{
		int num = Config.PrimaryAttributeBase + 5 * Mathf.RoundToInt((float)cr * 0.325f);
		return category switch
		{
			AttributeCategoryType.Dump => 30, 
			AttributeCategoryType.Tertiary => 5 * Mathf.CeilToInt((float)num * 0.13f), 
			AttributeCategoryType.Secondary => 5 * Mathf.CeilToInt((float)num * 0.162f), 
			AttributeCategoryType.Primary => num, 
			AttributeCategoryType.PrimaryPlus => num + Config.PrimaryPlusAttributeIncrease, 
			_ => throw new ArgumentOutOfRangeException("category", category, null), 
		};
	}

	private static float GetUnmodifiedHitPoints(BlueprintUnit blueprint, NPCDifficultyOption npcDifficulty, int cr)
	{
		return (float)Config.GetUnmodifiedWoundsBaseValue(blueprint.DifficultyType) * GetSynergyFactor(npcDifficulty, cr) * Math.Min(1f, 1f - 0.1f * (float)(8 - cr));
	}

	private static float GetSynergyFactor(NPCDifficultyOption difficulty, int cr)
	{
		float num = Math.Max(1f, 0.75f + (float)cr * 0.13f);
		return difficulty switch
		{
			NPCDifficultyOption.Story => num, 
			NPCDifficultyOption.Normal => GetSynergyFactorNormal(num), 
			NPCDifficultyOption.Daring => GetSynergyFactorDaring(num), 
			NPCDifficultyOption.Hard => GetSynergyFactorHard(num), 
			NPCDifficultyOption.Unfair => GetSynergyFactorUnfair(num), 
			_ => throw new ArgumentOutOfRangeException("difficulty", difficulty, null), 
		};
		static float GetFactorBase(float sf, float f)
		{
			return 1f + (sf - 1f) * f;
		}
		static float GetSynergyFactorDaring(float sf)
		{
			return GetFactorBase(sf, 0.75f) * GetFactorBase(sf, 0.25f);
		}
		static float GetSynergyFactorHard(float sf)
		{
			return GetFactorBase(sf, 0.5f) * GetFactorBase(sf, 0.3f) * GetFactorBase(sf, 0.2f);
		}
		static float GetSynergyFactorNormal(float sf)
		{
			return GetFactorBase(sf, 0.9f) * GetFactorBase(sf, 0.1f);
		}
		static float GetSynergyFactorUnfair(float sf)
		{
			float factorBase = GetFactorBase(sf, 0.25f);
			return factorBase * factorBase * factorBase * factorBase;
		}
	}

	private static void TryApplyAdvance(BlueprintUnit blueprint, Dictionary<AttributeType, AttributeCategory> attributeCategories, BlueprintUnitStatsRoot.AttributeCategoryAdvance advance, AttributeCategoryAdvanceType type)
	{
		if (blueprint.HasSuitableWeapon(advance.RequiredWeapon))
		{
			ApplyAdvance(attributeCategories, advance.Attribute, advance.Advance, type);
		}
	}

	private static void ApplyAdvance(Dictionary<AttributeType, AttributeCategory> attributeCategories, AttributeType attribute, int value, AttributeCategoryAdvanceType type)
	{
		if (!attributeCategories.TryGetValue(attribute, out var value2))
		{
			AttributeCategory attributeCategory2 = (attributeCategories[attribute] = new AttributeCategory());
			value2 = attributeCategory2;
		}
		value2.Add(type, value);
	}

	private static bool HasSuitableWeapon(this BlueprintUnit blueprint, BlueprintUnitStatsRoot.WeaponType weaponType)
	{
		if (weaponType != 0 && (weaponType != BlueprintUnitStatsRoot.WeaponType.Melee || !blueprint.HasMeleeWeapon()))
		{
			if (weaponType == BlueprintUnitStatsRoot.WeaponType.Ranged)
			{
				return blueprint.HasRangedWeapon();
			}
			return false;
		}
		return true;
	}

	private static bool HasMeleeWeapon(this BlueprintUnit blueprint)
	{
		return blueprint.Body.ItemEquipmentHandSettings.Contains((BlueprintItemWeapon i) => i.IsMelee);
	}

	private static bool HasRangedWeapon(this BlueprintUnit blueprint)
	{
		return blueprint.Body.ItemEquipmentHandSettings.Contains((BlueprintItemWeapon i) => i.IsRanged);
	}

	private static int RoundToFive(float value)
	{
		return Mathf.RoundToInt(value + 2f) / 5 * 5;
	}

	private static bool IsTough(this BlueprintUnit blueprint)
	{
		return Config.ToughSubtypes.HasItem(blueprint.Subtype);
	}

	private static bool IsFragile(this BlueprintUnit blueprint)
	{
		if (!Config.FragileSubtypes.HasItem(blueprint.Subtype))
		{
			if (blueprint.Subtype == UnitSubtype.Default)
			{
				UnitDifficultyType difficultyType = blueprint.DifficultyType;
				if (difficultyType == UnitDifficultyType.Common || difficultyType == UnitDifficultyType.Swarm)
				{
					return !blueprint.HasMeleeWeapon();
				}
			}
			return false;
		}
		return true;
	}

	private static bool IsTank(this BlueprintUnit blueprint)
	{
		return Config.TankSubtypes.HasItem(blueprint.Subtype);
	}
}
