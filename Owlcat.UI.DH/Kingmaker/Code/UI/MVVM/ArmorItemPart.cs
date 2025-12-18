using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ArmorItemPart : BaseItemPart
{
	private Dictionary<TooltipElement, Sprite> m_ElementsIcons = new Dictionary<TooltipElement, Sprite>
	{
		{
			TooltipElement.Durability,
			UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability
		},
		{
			TooltipElement.DamageReduction,
			UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction
		}
	};

	public ArmorItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public ArmorItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		string text = m_ItemTooltipData.GetText(TooltipElement.Subname);
		ItemEntity item = m_Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(m_BlueprintItem, null)?.Icon;
		bool hasUpgrade = m_BlueprintItem.PrototypeLink is BlueprintItemArmor;
		List<ArmourTagUISettings> tagSettings = (m_BlueprintItem as BlueprintItemArmor)?.ArmourTags.ToList() ?? new List<ArmourTagUISettings>();
		list.Add(new TooltipBrickArmourHeader(itemName, image, GetStatDataFor(TooltipElement.Durability), GetStatDataFor(TooltipElement.DamageReduction), hasUpgrade, tagSettings, text));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddArmorStats(list);
		AddItemStatBonuses(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	private void AddArmorStats(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddArmourTags(list);
		if (list.Any())
		{
			bricks.AddRange(list);
		}
	}

	private void AddArmourTags(List<ITooltipBrick> bricks)
	{
		if (!UIConfig.Instance.FeatureTagsConfig.ShowTagsDescriptions)
		{
			return;
		}
		List<ArmourTagUISettings> list = (m_BlueprintItem as BlueprintItemArmor)?.ArmourTags.ToList() ?? new List<ArmourTagUISettings>();
		if (!list.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickText(string.Empty));
		foreach (ArmourTagUISettings item in list)
		{
			if (item.OwnerBlueprint is BlueprintFeature)
			{
				Sprite armourTagIcon = UIConfig.Instance.FeatureTagsConfig.GetArmourTagIcon(item);
				Color armourMountColor = UIConfig.Instance.FeatureTagsConfig.GetArmourMountColor(item);
				TempTagUtils.GetTagNameAndDescription(item, out var tagName, out var tagDescription);
				bricks.Add(new TooltipBrickTagDescription(armourTagIcon, armourMountColor, tagName, tagDescription));
			}
		}
	}

	private StatData GetStatDataFor(TooltipElement type)
	{
		m_ElementsIcons.TryGetValue(type, out var value);
		string text = m_ItemTooltipData.GetText(type);
		string tooltipElementLabel = UIUtilityTooltip.GetTooltipElementLabel(type);
		ComparisonResult comparison = ComparisonResult.Equal;
		if (base.HasCompareItem)
		{
			CompareData compareData = m_ItemTooltipData.GetCompareData(type);
			CompareData compareData2 = m_CompareItemTooltipData.GetCompareData(type);
			comparison = CompareValues(compareData.Value, compareData2.Value);
		}
		return new StatData(highlight: (!m_ItemTooltipData.GetHasValue(type)) ? StatData.StatHighlight.Negative : StatData.StatHighlight.Default, value: text, label: tooltipElementLabel, icon: value, comparison: comparison);
	}
}
