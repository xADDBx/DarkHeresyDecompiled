using System;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class CharInfoAbilityStatVMFactory
{
	private static readonly EnumWeaponStatIcons Icons = UIConfig.Instance.WeaponStatIcons;

	private static readonly StatsStrings Stats = LocalizedTexts.Instance.Stats;

	public static CharInfoAbilityStatVM TryCreate(WeaponStat stat, BlueprintAbility ability, ItemEntity item, MechanicEntity caster)
	{
		AbilityData abilityData = UIUtilityItem.CreateAbilityData(ability, item, caster);
		string value = GetValue(stat, caster, abilityData);
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		Sprite sprite = Icons.GetSprite(stat);
		TooltipBaseTemplate tooltip = GetTooltip(stat, caster, abilityData);
		return new CharInfoAbilityStatVM(value, sprite, tooltip);
	}

	private static string GetValue(WeaponStat stat, MechanicEntity caster, AbilityData abilityData)
	{
		return stat switch
		{
			WeaponStat.ShotHitChance => CalculateHitChance(caster, abilityData), 
			WeaponStat.StrikeHitChance => CalculateHitChance(caster, abilityData), 
			WeaponStat.PreciseHitChance => CalculateHitChance(caster, abilityData), 
			WeaponStat.AmmoCount => GetAmmoCount(abilityData), 
			WeaponStat.AttacksCount => GetAttackCount(abilityData), 
			WeaponStat.AdditionalArmorDamage => GetArmorDamageBonus(abilityData.SourceItem), 
			WeaponStat.AdditionalWoundsDamage => GetWoundDamageBonus(abilityData.SourceItem), 
			WeaponStat.None => string.Empty, 
			_ => string.Empty, 
		};
	}

	private static TooltipBaseTemplate GetTooltip(WeaponStat stat, MechanicEntity caster, AbilityData abilityData)
	{
		switch (stat)
		{
		case WeaponStat.ShotHitChance:
		case WeaponStat.StrikeHitChance:
		case WeaponStat.PreciseHitChance:
			return GetHitChanceTooltip(stat, caster, abilityData);
		case WeaponStat.AmmoCount:
		case WeaponStat.AttacksCount:
		case WeaponStat.AdditionalArmorDamage:
		case WeaponStat.AdditionalWoundsDamage:
			return new TooltipTemplateSimple(Stats.GetText(stat));
		default:
			return null;
		}
	}

	private static TooltipBaseTemplate GetHitChanceTooltip(WeaponStat stat, MechanicEntity caster, AbilityData abilityData)
	{
		string text = Stats.GetText(stat);
		RuleCalculateHitChances hitChanceRoll = UIUtilityItem.GetHitChanceRoll(caster, abilityData);
		StringBuilder stringBuilder = new StringBuilder();
		if (hitChanceRoll != null)
		{
			stringBuilder.Append($"{Stats.GetText(hitChanceRoll.ResultAttackStatType)}: <b>{hitChanceRoll.ResultAttackStatValue}</b>\n");
			foreach (Modifier sortedModifiers in hitChanceRoll.Modifiers.SortedModifiersList)
			{
				stringBuilder.Append(StatModifiersBreakdown.GetBonusSourceText(sortedModifiers) + ": <b>" + UIUtilityItem.AddSignToModifierText(sortedModifiers.Value, sortedModifiers.Type) + "</b>\n");
			}
		}
		return new TooltipTemplateSimple(text, stringBuilder.ToString());
	}

	private static string CalculateHitChance(MechanicEntity caster, AbilityData abilityData)
	{
		RuleCalculateHitChances hitChanceRoll = UIUtilityItem.GetHitChanceRoll(caster, abilityData);
		if (hitChanceRoll != null)
		{
			return UIUtilityText.GetPercentString(hitChanceRoll.ResultHitChance);
		}
		return string.Empty;
	}

	[Obsolete("WH2-7361")]
	private static string GetAmmoCount(AbilityData abilityData)
	{
		return string.Empty;
	}

	private static string GetAttackCount(AbilityData abilityData)
	{
		return "1";
	}

	public static string GetArmorDamageBonus(ItemEntity item)
	{
		return UIUtilityItem.GetArmorDamageBonus(item);
	}

	public static string GetWoundDamageBonus(ItemEntity item)
	{
		return UIUtilityItem.GetWoundDamageBonus(item);
	}

	public static string GetVitalDamageBonus(ItemEntity item)
	{
		return UIUtilityItem.GetVitalDamageBonus(item);
	}
}
