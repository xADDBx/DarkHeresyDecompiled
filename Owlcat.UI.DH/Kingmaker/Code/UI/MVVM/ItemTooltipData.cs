using System.Collections.Generic;
using Code.View.UI.MVVM.Tooltip;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class ItemTooltipData
{
	public ItemEntity Item;

	public ItemEntityUsable Usable;

	public BlueprintItem BlueprintItem;

	public BlueprintItemEquipmentUsable BlueprintUsable;

	public readonly Dictionary<DamageType, string> OtherDamage = new Dictionary<DamageType, string>();

	public readonly Dictionary<StatType, int> StatBonus = new Dictionary<StatType, int>();

	public readonly List<RestrictionData> Restrictions = new List<RestrictionData>();

	public BlueprintAbility BlueprintAbility;

	public Dictionary<TooltipElement, string> Texts { get; } = new Dictionary<TooltipElement, string>();


	public Dictionary<TooltipElement, string> AddTexts { get; } = new Dictionary<TooltipElement, string>();


	public Dictionary<TooltipElement, bool> HasValues { get; } = new Dictionary<TooltipElement, bool>();


	public Dictionary<TooltipElement, CompareData> CompareData { get; } = new Dictionary<TooltipElement, CompareData>();


	public Dictionary<StatType, string> BonusDamageFromStat { get; } = new Dictionary<StatType, string>();


	public List<UIUtilityItem.UIAbilityData> Abilities { get; } = new List<UIUtilityItem.UIAbilityData>();


	public ItemTooltipData(ItemEntity item)
	{
		Item = item;
		Usable = item as ItemEntityUsable;
	}

	public ItemTooltipData(BlueprintItem blueprintItem)
	{
		BlueprintItem = blueprintItem;
		BlueprintUsable = blueprintItem as BlueprintItemEquipmentUsable;
	}

	public string GetText(TooltipElement type)
	{
		return Texts.Get(type);
	}

	public string GetAddText(TooltipElement type)
	{
		return AddTexts.Get(type);
	}

	public bool GetHasValue(TooltipElement type)
	{
		return HasValues.Get(type, defaultValue: true);
	}

	public CompareData GetCompareData(TooltipElement type)
	{
		return CompareData.Get(type);
	}

	public string GetOtherDamage(DamageType type)
	{
		return OtherDamage.Get(type);
	}
}
