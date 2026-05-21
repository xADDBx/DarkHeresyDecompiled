using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

[UsedImplicitly]
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
		},
		{
			TooltipElement.Defense,
			UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence
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
		BlueprintItemArmor blueprintItemArmor = m_BlueprintItem as BlueprintItemArmor;
		bool hasUpgrade = m_BlueprintItem.PrototypeLink is BlueprintItemArmor;
		List<ArmourTagUISettings> tagSettings = blueprintItemArmor?.ArmourTags.ToList() ?? new List<ArmourTagUISettings>();
		string itemLabel = ((blueprintItemArmor != null && blueprintItemArmor.Category != 0) ? UIUtilityText.WrapWithWeight(LocalizedTexts.Instance.Stats.GetText(blueprintItemArmor.Category), TextFontWeight.SemiBold) : null);
		StatData armourDefence = (m_ItemTooltipData.GetHasValue(TooltipElement.Defense) ? GetStatDataFor(TooltipElement.Defense) : null);
		list.Add(new BrickArmourHeaderVM(itemName, image, GetStatDataFor(TooltipElement.Durability), GetStatDataFor(TooltipElement.DamageReduction), hasUpgrade, tagSettings, m_BlueprintItem, text, itemLabel, armourDefence));
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
		AddArtisticDescription(list);
		return list;
	}

	private void AddArmorStats(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddMaxDefence(list);
		AddArmourTags(list);
		if (list.Any())
		{
			bricks.AddRange(list);
		}
	}

	private void AddMaxDefence(List<ITooltipBrick> bricks)
	{
		int num = (m_BlueprintItem as BlueprintItemArmor)?.MaxDefence ?? 0;
		if (num > 0)
		{
			string text = UIStrings.Instance.Tooltips.MaxDefence;
			string value = UIUtilityText.AddPercentTo(num);
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(text, value)));
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
		bricks.Add(new BrickTextVM(string.Empty));
		foreach (ArmourTagUISettings tag in list)
		{
			if (tag.OwnerBlueprint is BlueprintFeature)
			{
				Sprite armourTagIcon = UIConfig.Instance.FeatureTagsConfig.GetArmourTagIcon(tag);
				Color armourMountColor = UIConfig.Instance.FeatureTagsConfig.GetArmourMountColor(tag);
				string descriptionWithItemEquipped = UIUtilityItem.GetDescriptionWithItemEquipped(m_Item, () => UIUtilityItem.GetTagName(tag));
				string descriptionWithItemEquipped2 = UIUtilityItem.GetDescriptionWithItemEquipped(m_Item, () => UIUtilityItem.GetTagDescription(tag));
				bricks.Add(new BrickSpaceVM(25f));
				bricks.Add(new BrickTagDescriptionVM(armourTagIcon, armourMountColor, descriptionWithItemEquipped, descriptionWithItemEquipped2));
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
		return new StatData(highlight: (!m_ItemTooltipData.GetHasValue(type)) ? StatData.StatHighlight.Negative : StatData.StatHighlight.Default, text: new TextValueElement(tooltipElementLabel, text), icon: value, comparison: comparison);
	}
}
