using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

[UsedImplicitly]
public class WeaponItemPart : BaseItemPart
{
	public BlueprintItemWeapon BlueprintItemWeapon => (BlueprintItemWeapon)m_BlueprintItem;

	public WeaponItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public WeaponItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
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

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDOT(list);
		AddWeaponStats(list);
		AddCombatVeterancy(list);
		AddAbilities(list);
		AddItemStatBonuses(list);
		AddWeaponTags(list, type);
		AddDescription(list, type);
		AddArtisticDescription(list);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	private ITooltipBrick CreateWeaponHeader()
	{
		string itemName = GetItemName();
		StatData damageStatData = GetDamageStatData();
		string itemType = (BlueprintItemWeapon.IsTwoHanded ? UIStrings.Instance.InventoryScreen.TwoHandWeapon.Text : UIStrings.Instance.InventoryScreen.OneHandWeapon.Text);
		string weaponRangeLabel = UIStrings.Instance.WeaponCategories.GetWeaponRangeLabel(BlueprintItemWeapon.Range);
		string text = UIUtilityText.WrapWithWeight(UIStrings.Instance.WeaponCategories.GetWeaponHeavinessLabel(BlueprintItemWeapon.Heaviness), TextFontWeight.SemiBold) ?? "";
		if (!string.IsNullOrEmpty(weaponRangeLabel))
		{
			text = text + " | " + weaponRangeLabel;
		}
		string text2 = m_ItemTooltipData.GetText(TooltipElement.WeaponFamily);
		ItemEntity item = m_Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(m_BlueprintItem, null)?.Icon;
		return new BrickWeaponHeaderVM(itemName, image, damageStatData, hasUpgrade: false, BlueprintItemWeapon.WeaponTags.ToList(), itemType, text, text2, BlueprintItemWeapon, m_Item as ItemEntityWeapon);
	}

	private void AddWeaponTags(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (!UIConfig.Instance.FeatureTagsConfig.ShowTagsDescriptions)
		{
			return;
		}
		List<WeaponTagUISettings> source = BlueprintItemWeapon?.WeaponTags.ToList() ?? new List<WeaponTagUISettings>();
		if (!source.Any())
		{
			return;
		}
		bricks.Add(new BrickTextVM(string.Empty));
		foreach (WeaponTagUISettings tag in source.OrderByDescending((WeaponTagUISettings t) => t.Type))
		{
			if (!tag.IsBodyIgnoreTag())
			{
				Sprite weaponTagIcon = UIConfig.Instance.FeatureTagsConfig.GetWeaponTagIcon(tag);
				Color weaponMountColor = UIConfig.Instance.FeatureTagsConfig.GetWeaponMountColor(tag);
				string descriptionWithItemEquipped = UIUtilityItem.GetDescriptionWithItemEquipped(m_Item, () => UIUtilityItem.GetTagName(tag));
				string descriptionWithItemEquipped2 = UIUtilityItem.GetDescriptionWithItemEquipped(m_Item, () => UIUtilityItem.GetTagDescription(tag));
				bricks.Add(new BrickSpaceVM(25f));
				bricks.Add(new BrickTagDescriptionVM(weaponTagIcon, weaponMountColor, descriptionWithItemEquipped, descriptionWithItemEquipped2));
			}
		}
	}

	protected override void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (!(BlueprintItemWeapon?.WeaponTags.ToList() ?? new List<WeaponTagUISettings>()).Any((WeaponTagUISettings t) => t.Type == PropertyType.Unique))
		{
			base.AddDescription(bricks, type);
		}
	}

	protected override CostStruct GetCost()
	{
		CostStruct cost = base.GetCost();
		WeaponTagUISettings weaponTagUISettings = BlueprintItemWeapon?.WeaponTags.FirstOrDefault((WeaponTagUISettings t) => t.Tag == WeaponTagProperty.Expensive);
		if (weaponTagUISettings == null)
		{
			return cost;
		}
		string tagName = UIUtilityItem.GetTagName(weaponTagUISettings);
		string tagDescription = UIUtilityItem.GetTagDescription(weaponTagUISettings);
		return new CostStruct(cost.LeftText, cost.RightText, cost.AdditionalText, CostType.Expensive, new TooltipTemplateSimple(tagName, tagDescription));
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
		AddIconStatValue(bricks, TooltipElement.OverpenetrationChance, null, BrickElementPalette.Positive, BrickElementPalette.Positive);
		AddIconStatValue(bricks, TooltipElement.HitChance, null, BrickElementPalette.Positive, BrickElementPalette.Positive);
	}

	private void AddRateOfFire(List<ITooltipBrick> bricks)
	{
		AddIconStatValue(bricks, TooltipElement.Recoil, ConfigRoot.Instance.UIConfig.UIIcons.Recoil);
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
			bricks.Add(new BrickWeaponDOTInitialDamageVM(buff, num));
		}
	}

	private void AddCombatVeterancy(List<ITooltipBrick> bricks)
	{
		if (!(m_Item is ItemEntityWeapon { Wielder: BaseUnitEntity { IsPlayerEnemy: not false } wielder }))
		{
			return;
		}
		float damageFactor = DifficultyUtils.GetDamageFactor(SettingsRoot.Difficulty.EnemyDamage, wielder.CR);
		if (!Mathf.Approximately(damageFactor, 1f))
		{
			string text = LocalizedTexts.Instance.Descriptors.GetText(ModifierDescriptor.EnemyCombatVeterancy);
			if (string.IsNullOrEmpty(text))
			{
				text = "Combat Veterancy";
			}
			bricks.Add(new BrickTextVM($"{text}: ×{damageFactor:F2}"));
		}
	}
}
