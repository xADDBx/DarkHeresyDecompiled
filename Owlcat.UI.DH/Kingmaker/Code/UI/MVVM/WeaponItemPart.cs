using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponItemPart : BaseItemPart
{
	public WeaponItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		list.Add(CreateWeaponHeader());
		return list;
	}

	private ITooltipBrick CreateWeaponHeader()
	{
		string itemName = GetItemName();
		StatData damageStatData = GetDamageStatData();
		BlueprintItemWeapon blueprintItemWeapon = (BlueprintItemWeapon)m_BlueprintItem;
		string itemType = (blueprintItemWeapon.IsTwoHanded ? UIStrings.Instance.InventoryScreen.TwoHandWeapon.Text : UIStrings.Instance.InventoryScreen.OneHandWeapon.Text);
		string itemLabel = string.Concat(str2: UIStrings.Instance.WeaponCategories.GetWeaponRangeLabel(blueprintItemWeapon.Range), str0: UIUtilityText.WrapWithWeight(UIStrings.Instance.WeaponCategories.GetWeaponHeavinessLabel(blueprintItemWeapon.Heaviness), TextFontWeight.SemiBold), str1: " | ");
		string text = m_ItemTooltipData.GetText(TooltipElement.WeaponFamily);
		ItemEntity item = m_Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(m_BlueprintItem, null)?.Icon;
		bool hasUpgrade = false;
		return new TooltipBrickWeaponHeader(itemName, image, damageStatData, hasUpgrade, blueprintItemWeapon.WeaponTags.ToList(), itemType, itemLabel, text, GetSpecialValues());
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDOT(list);
		AddWeaponStats(list);
		AddAbilities(list);
		AddItemStatBonuses(list);
		AddWeaponTags(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	private void AddWeaponTags(List<ITooltipBrick> bricks)
	{
		if (!UIConfig.Instance.FeatureTagsConfig.ShowTagsDescriptions)
		{
			return;
		}
		List<WeaponTagUISettings> list = (m_BlueprintItem as BlueprintItemWeapon)?.WeaponTags.ToList() ?? new List<WeaponTagUISettings>();
		if (!list.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickText(string.Empty));
		foreach (WeaponTagUISettings item in list)
		{
			if (!TooltipBrickWeaponHeaderVM.SpecialTags.Contains(item.Tag))
			{
				Sprite weaponTagIcon = UIConfig.Instance.FeatureTagsConfig.GetWeaponTagIcon(item);
				Color weaponMountColor = UIConfig.Instance.FeatureTagsConfig.GetWeaponMountColor(item);
				TempTagUtils.GetTagNameAndDescription(item, out var tagName, out var tagDescription);
				tagDescription = UIUtilityText.UpdateDescriptionWithUIProperties(tagDescription, m_Item.Owner);
				bricks.Add(new TooltipBrickTagDescription(weaponTagIcon, weaponMountColor, tagName, tagDescription));
			}
		}
	}

	protected override void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (!((m_BlueprintItem as BlueprintItemWeapon)?.WeaponTags.ToList() ?? new List<WeaponTagUISettings>()).Any((WeaponTagUISettings t) => t.Type == PropertyType.Unique))
		{
			base.AddDescription(bricks, type);
		}
	}

	private void AddWeaponStats(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddAdditionalHitChance(list);
		AddRateOfFire(list);
		AddRange(list);
		if (list.Any())
		{
			bricks.AddRange(list);
		}
	}

	private void AddAdditionalHitChance(List<ITooltipBrick> bricks)
	{
		AddIconStatValue(bricks, TooltipElement.SingleAdditionalHitChance, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive);
		AddIconStatValue(bricks, TooltipElement.BurstAdditionalHitChance, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive);
		AddIconStatValue(bricks, TooltipElement.AoeAdditionalHitChance, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive);
	}

	private void AddRateOfFire(List<ITooltipBrick> bricks)
	{
		if (!(m_Item is ItemEntityWeapon itemEntityWeapon) || itemEntityWeapon.Blueprint.IsRanged)
		{
			Sprite crit = ConfigRoot.Instance.UIConfig.UIIcons.Crit;
			AddIconStatValue(bricks, TooltipElement.RateOfFire, crit);
		}
	}

	private void AddRange(List<ITooltipBrick> bricks)
	{
		Sprite range = ConfigRoot.Instance.UIConfig.UIIcons.Range;
		AddIconStatValue(bricks, TooltipElement.MaxDistance, range);
	}

	private void AddDOT(List<ITooltipBrick> bricks)
	{
		if (!(m_BlueprintItem is BlueprintItemWeapon blueprint))
		{
			return;
		}
		BlueprintUnitFact blueprintUnitFact = blueprint.GetComponent<AddFactToEquipmentWielder>()?.Fact;
		if (blueprintUnitFact == null)
		{
			return;
		}
		AbilityLifecycleTriggerCaster component = blueprintUnitFact.GetComponent<AbilityLifecycleTriggerCaster>();
		if (component == null)
		{
			return;
		}
		int num = -1;
		BlueprintBuff buff = null;
		foreach (BlueprintMechanicEntityFact fact in component.Facts)
		{
			if (!(fact is BlueprintBuff blueprint2))
			{
				continue;
			}
			BuffVisualPart component2 = blueprint2.GetComponent<BuffVisualPart>();
			if (component2 != null)
			{
				ContextStackingUnitProperty component3 = blueprint2.GetComponent<ContextStackingUnitProperty>();
				if (component3 != null)
				{
					buff = component2.Buff;
					num = component3.PropertyValue.Value;
					break;
				}
			}
		}
		if (num > 0)
		{
			bricks.Add(new TooltipBrickWeaponDOTInitialDamage(buff, num));
		}
	}

	private Dictionary<WeaponTagProperty, int> GetSpecialValues()
	{
		BlueprintItemWeapon blueprintItemWeapon = (BlueprintItemWeapon)m_BlueprintItem;
		return new Dictionary<WeaponTagProperty, int>
		{
			{
				WeaponTagProperty.Brutal,
				blueprintItemWeapon.BrutalDamage
			},
			{
				WeaponTagProperty.Destructive,
				blueprintItemWeapon.DestructiveDamage
			}
		};
	}
}
