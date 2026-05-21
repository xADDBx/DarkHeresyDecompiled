using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Critters;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[UsedImplicitly]
public class BaseItemPart : TooltipBaseTemplate
{
	protected readonly ItemEntity m_Item;

	protected readonly BlueprintItem m_BlueprintItem;

	protected readonly ItemTooltipData m_ItemTooltipData;

	protected readonly ItemTooltipData m_CompareItemTooltipData;

	protected bool HasCompareItem => m_CompareItemTooltipData != null;

	public BaseItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool _ = false)
	{
		m_Item = item;
		m_BlueprintItem = item.Blueprint;
		m_ItemTooltipData = itemTooltipData;
		m_CompareItemTooltipData = compareItemTooltipData;
		ContentSpacing = 0f;
	}

	public BaseItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool _ = false)
	{
		m_BlueprintItem = blueprintItem;
		m_ItemTooltipData = itemTooltipData;
		m_CompareItemTooltipData = compareItemTooltipData;
		ContentSpacing = 0f;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		string text = m_ItemTooltipData.GetText(TooltipElement.Subname);
		ItemEntity item = m_Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(m_BlueprintItem, null)?.Icon;
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		list.Add(new BrickEntityHeaderVM(itemName, image, hasUpgrade: false, text));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddNotRemovable(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		AddArtisticDescription(list);
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddEndTurnInfo(list, type);
		AddCost(list);
		return list;
	}

	protected string GetItemName()
	{
		StringBuilder stringBuilder = new StringBuilder(m_ItemTooltipData.GetText(TooltipElement.Name));
		ItemEntity item = m_Item;
		if (item != null && item.Count > 1)
		{
			stringBuilder.Append($" (x{m_Item?.Count})");
		}
		return stringBuilder.ToString();
	}

	protected void AddIconStatValue(List<ITooltipBrick> bricks, TooltipElement element, Sprite icon = null, BrickElementPalette type = BrickElementPalette.Normal, BrickElementPalette bgrType = BrickElementPalette.Normal)
	{
		string text = m_ItemTooltipData.GetText(element);
		string addText = m_ItemTooltipData.GetAddText(element);
		string tooltipElementLabel = UIUtilityTooltip.GetTooltipElementLabel(element);
		AddIconStatValue(bricks, tooltipElementLabel, text, addText, icon, type, bgrType);
	}

	private void AddIconStatValue(List<ITooltipBrick> bricks, string label, string value, string addValue, Sprite icon, BrickElementPalette type = BrickElementPalette.Normal, BrickElementPalette bgrType = BrickElementPalette.Normal)
	{
		if (!string.IsNullOrEmpty(value))
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(label, value, addValue), icon, type, bgrType));
		}
	}

	protected ComparisonResult CompareValues(int baseValue, int otherValue)
	{
		if (baseValue > otherValue)
		{
			return ComparisonResult.Greater;
		}
		if (baseValue < otherValue)
		{
			return ComparisonResult.Less;
		}
		return ComparisonResult.Equal;
	}

	protected void AddDamage(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(UIUtilityText.AddPercentTo(m_ItemTooltipData.GetText(TooltipElement.Penetration))) && !string.IsNullOrEmpty(GetDamageText()))
		{
			StatData damageStatData = GetDamageStatData();
			bricks.Add(new BrickTwoColumnsStatVM(damageStatData));
		}
	}

	protected StatData GetDamageStatData()
	{
		TooltipElement type = (m_ItemTooltipData.GetText(TooltipElement.EquipDamage).Empty() ? TooltipElement.Damage : TooltipElement.EquipDamage);
		string damageText = GetDamageText();
		ComparisonResult comparison = ComparisonResult.Equal;
		if (HasCompareItem)
		{
			CompareData compareData = m_ItemTooltipData.GetCompareData(type);
			CompareData compareData2 = m_CompareItemTooltipData.GetCompareData(type);
			comparison = CompareValues(compareData.Value, compareData2.Value);
		}
		StatData.StatHighlight highlight = StatData.StatHighlight.Default;
		if (m_BlueprintItem.PrototypeLink is BlueprintItemWeapon { CanBeUsedInGame: not false } blueprintItemWeapon && m_BlueprintItem is BlueprintItemWeapon blueprintItemWeapon2)
		{
			highlight = ((((m_Item as ItemEntityWeapon)?.DamageMin ?? blueprintItemWeapon2.GetDamageMin()) > blueprintItemWeapon.GetDamageMin()) ? StatData.StatHighlight.Positive : StatData.StatHighlight.Default);
		}
		return new StatData(new TextValueElement(UIUtilityTooltip.GetTooltipElementLabel(TooltipElement.Damage), damageText), null, comparison, highlight);
	}

	private string GetDamageText()
	{
		TooltipElement type = (m_ItemTooltipData.GetText(TooltipElement.EquipDamage).Empty() ? TooltipElement.Damage : TooltipElement.EquipDamage);
		string text = m_ItemTooltipData.GetText(TooltipElement.BaseDamage);
		string text2 = m_ItemTooltipData.GetText(type);
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		return text2;
	}

	protected void AddRestrictions(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip)
		{
			AddTooltipRestrictions(bricks);
			return;
		}
		if (m_ItemTooltipData.Restrictions.Any())
		{
			bricks.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = m_BlueprintItem.IsFamiliarItem();
		for (int i = 0; i < m_ItemTooltipData.Restrictions.Count; i++)
		{
			RestrictionData restrictionData = m_ItemTooltipData.Restrictions[i];
			if (flag)
			{
				bricks.Add((type == TooltipTemplateType.Info) ? new BrickSeparatorVM() : new BrickSeparatorVM(TooltipBrickElementType.Small));
				flag = false;
			}
			if (restrictionData.Inverted)
			{
				if ((type == TooltipTemplateType.Info || restrictionData.RestrictionItems.Any((RestrictionItem item) => !item.MeetPrerequisite)) && !flag2 && !flag4)
				{
					bricks.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
					flag2 = true;
					flag3 = false;
				}
			}
			else if (type == TooltipTemplateType.Info && !flag3 && !flag4)
			{
				bricks.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.PrerequisiteFeatures, TooltipTitleType.H3));
				flag2 = false;
				flag3 = true;
			}
			bool flag5 = false;
			for (int j = 0; j < restrictionData.RestrictionItems.Count; j++)
			{
				RestrictionItem restrictionItem = restrictionData.RestrictionItems[j];
				if (restrictionItem.MeetPrerequisite && type != TooltipTemplateType.Info)
				{
					continue;
				}
				if (flag5)
				{
					if (restrictionData.Inverted)
					{
						bricks.Add((type == TooltipTemplateType.Info) ? new BrickSeparatorVM() : new BrickSeparatorVM(TooltipBrickElementType.Small));
					}
					else
					{
						int num = ((type == TooltipTemplateType.Info) ? 100 : UIConfig.Instance.SubTextPercentSize);
						bricks.Add(new BrickItemHeaderVM($"<size={num}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
					}
				}
				if (restrictionItem.UnitFact != null)
				{
					if (restrictionItem.UnitFact is BlueprintFeature feature)
					{
						bricks.Add(new BrickFeatureVM(feature, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info, null, null, forceSetName: true));
					}
					else
					{
						bricks.Add(new BrickFeatureVM(restrictionItem.UnitFact.Name, null, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info));
					}
				}
				else
				{
					BrickElementPalette brickElementPalette = ((!restrictionItem.MeetPrerequisite) ? BrickElementPalette.Negative : BrickElementPalette.Normal);
					bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(restrictionItem.Key, restrictionItem.Value), null, brickElementPalette, brickElementPalette));
				}
				flag5 = true;
				flag = true;
			}
		}
	}

	private void AddTooltipRestrictions(List<ITooltipBrick> bricks)
	{
		if (UIUtilityItem.CanInsertItem(m_Item) || m_Item is ItemEntityArmor)
		{
			bool canEquip = !UIUtilityCombat.IsCombatLockActive();
			string ownerName = ((HasCompareItem && m_Item?.Owner is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.CharacterName : null);
			bricks.Add(new BrickItemRestrictionVM(m_ItemTooltipData.Restrictions, canEquip, ownerName));
		}
	}

	protected void AddAbilities(List<ITooltipBrick> bricks)
	{
		if (!m_ItemTooltipData.Abilities.Any())
		{
			return;
		}
		foreach (UIUtilityItem.UIAbilityData ability in m_ItemTooltipData.Abilities)
		{
			TooltipTemplateAbility tooltip = new TooltipTemplateAbility(ability.BlueprintAbility, m_BlueprintItem);
			TextEntity title = new TextEntity(ability.Name);
			TextValueElement secondaryValuesElement = new TextValueElement(UIStrings.Instance.Tooltips.CostAP.Text, ability.CostAP);
			bricks.Add(new BrickIconPatternVM(ability.Icon, ability.PatternData, title, secondaryValuesElement, null, tooltip));
			if (!ability.UIProperties.Any())
			{
				continue;
			}
			List<TooltipBrickVM> list = new List<TooltipBrickVM>();
			foreach (UIProperty uIProperty in ability.UIProperties)
			{
				list.Add(new BrickIconStatValueVM(new TextValueAddElement(uIProperty.Name, uIProperty.PropertyValue?.ToString() ?? string.Empty, uIProperty.Description)));
			}
		}
	}

	protected void AddItemStatBonuses(List<ITooltipBrick> bricks)
	{
		if (m_ItemTooltipData.StatBonus.Empty())
		{
			return;
		}
		foreach (KeyValuePair<StatType, int> bonus in m_ItemTooltipData.StatBonus)
		{
			LocalizedString localizedString = ConfigRoot.Instance.LocalizedTexts.Stats.Entries.FirstOrDefault((Entry e) => e.Stat == bonus.Key)?.Text;
			string text = ((localizedString != null) ? ((string)localizedString) : "");
			BrickElementPalette type = ((bonus.Value > 0) ? BrickElementPalette.Positive : BrickElementPalette.Negative);
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(text, UIUtilityText.AddSign(bonus.Value)), null, type));
		}
	}

	private void AddNotRemovable(List<ITooltipBrick> bricks)
	{
		ItemEntity item = m_Item;
		if (item != null && item.IsNonRemovable)
		{
			bricks.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.IsNotRemovable, TooltipTitleType.H5));
		}
	}

	protected void AddReplenishing(List<ITooltipBrick> bricks)
	{
		string text = m_ItemTooltipData.GetText(TooltipElement.Replenishing);
		if (!string.IsNullOrEmpty(text))
		{
			bricks.Add(new BrickTextVM(text, TooltipTextType.Italic, TooltipTextAlignment.Midl, m_Item?.Owner));
		}
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string text = m_ItemTooltipData.GetText(TooltipElement.ShortDescription);
		string text2 = m_ItemTooltipData.GetText(TooltipElement.Description) + m_ItemTooltipData.GetText(TooltipElement.LongDescription);
		List<string> additionalDescription = TooltipTemplateUtils.GetAdditionalDescription(m_BlueprintItem);
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			string text4 = text;
			if (string.IsNullOrEmpty(text4))
			{
				text4 = text2;
				if (string.IsNullOrEmpty(text4) && additionalDescription.Count == 0)
				{
					bricks.Add(new BrickTitleVM(string.Empty, TooltipTitleType.H6));
					return;
				}
			}
			text4 = TooltipTemplateUtils.AggregateDescription(text4, additionalDescription);
			bricks.Add(new BrickTextVM(text4, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Item?.Owner));
			break;
		}
		case TooltipTemplateType.Info:
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = TooltipTemplateUtils.AggregateDescription(text2, additionalDescription);
				bricks.Add(new BrickTextVM(text2, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Item?.Owner));
			}
			else if (!string.IsNullOrEmpty(text))
			{
				text = TooltipTemplateUtils.AggregateDescription(text, additionalDescription);
				bricks.Add(new BrickTextVM(text, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Item?.Owner));
			}
			else if (additionalDescription.Count > 0)
			{
				string text3 = TooltipTemplateUtils.AggregateDescription("", additionalDescription);
				bricks.Add(new BrickTextVM(text3, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Item?.Owner));
			}
			break;
		}
		ItemEntity item = m_Item;
		if (item != null && item.HasUniqueSourceDescription)
		{
			bricks.Add(new BrickTextVM(m_Item?.UniqueSourceDescription ?? "", TooltipTextType.Italic, TooltipTextAlignment.Midl, m_Item?.Owner));
		}
	}

	protected void AddArtisticDescription(List<ITooltipBrick> bricks)
	{
		string text = m_ItemTooltipData.GetText(TooltipElement.ArtisticDescription);
		if (!string.IsNullOrEmpty(text))
		{
			bricks.Add(new BrickSeparatorVM());
			bricks.Add(new BrickSpaceVM());
			bricks.Add(new BrickTextVM(text, TooltipTextType.Italic, TooltipTextAlignment.Midl, m_Item?.Owner));
		}
	}

	private void AddEndTurnInfo(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string endTurn = GetEndTurn(m_BlueprintItem);
		Sprite moveEndPoints = UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints;
		string attackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(m_BlueprintItem);
		Sprite actionEndPoints = UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints;
		if (type == TooltipTemplateType.Info)
		{
			if (!string.IsNullOrEmpty(endTurn) || !string.IsNullOrEmpty(attackAbilityGroupCooldown))
			{
				bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Medium));
			}
			if (!string.IsNullOrEmpty(endTurn))
			{
				bricks.Add(new BrickIconValueStatVM(new TextValueElement(endTurn), moveEndPoints, IconColor.White, TextColor.Important));
			}
			if (!string.IsNullOrEmpty(attackAbilityGroupCooldown))
			{
				if (!string.IsNullOrEmpty(endTurn))
				{
					bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
				}
				bricks.Add(new BrickIconValueStatVM(new TextValueElement(attackAbilityGroupCooldown), actionEndPoints, IconColor.White, TextColor.Important));
			}
		}
		else
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			if (!string.IsNullOrWhiteSpace(endTurn) || !string.IsNullOrWhiteSpace(attackAbilityGroupCooldown))
			{
				TextFieldParams strikethrough = TextFieldParams.Strikethrough;
				list.Add(new BrickMultipleTextVM(new MultipleTextData(new TextEntity(endTurn, strikethrough), moveEndPoints), new MultipleTextData(new TextEntity(attackAbilityGroupCooldown, strikethrough), actionEndPoints)));
			}
			if (list.Count > 0)
			{
				bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Medium));
				bricks.AddRange(list);
			}
		}
	}

	private void AddCost(List<ITooltipBrick> bricks)
	{
		if (m_Item != null)
		{
			CostStruct cost = GetCost();
			bricks.Add(new BrickItemCostVM(cost));
		}
	}

	protected virtual CostStruct GetCost()
	{
		LocalizedString localizedString = (m_Item.IsFromVendorSlot() ? UIStrings.Instance.InventoryScreen.Buy : UIStrings.Instance.InventoryScreen.Sell);
		int num = Mathf.Max(1, m_Item.Count);
		int itemCost = Game.Instance.TradeLogic.GetItemCost(m_Item);
		string additionalText = string.Empty;
		if (num > 1)
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.Count = num;
				GameLogContext.Price = itemCost;
				additionalText = UIStrings.Instance.InventoryScreen.StackPrice.Text;
			}
		}
		return new CostStruct(localizedString, (itemCost * num).ToString(), additionalText, CostType.Default);
	}

	private string GetEndTurn(BlueprintItem blueprintItem)
	{
		if (!((blueprintItem?.GetComponent<AddFactToEquipmentWielder>())?.Fact is BlueprintAbility blueprint))
		{
			return string.Empty;
		}
		EndTurn component = blueprint.GetComponent<EndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPointsShort : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private string GetAttackAbilityGroupCooldown(BlueprintItem blueprintItem)
	{
		if (!((blueprintItem?.GetComponent<AddFactToEquipmentWielder>())?.Fact is BlueprintAbility blueprintAbility))
		{
			return string.Empty;
		}
		bool flag = false;
		foreach (BlueprintAbilityGroup abilityGroup in blueprintAbility.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.AttackAbilityGroupCooldownShort;
	}
}
