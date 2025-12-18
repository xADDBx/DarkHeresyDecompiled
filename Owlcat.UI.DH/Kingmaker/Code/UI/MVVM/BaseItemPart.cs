using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.MVVM.Tooltip;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Critters;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BaseItemPart : TooltipBaseTemplate
{
	protected readonly ItemEntity m_Item;

	protected readonly BlueprintItem m_BlueprintItem;

	protected readonly ItemTooltipData m_ItemTooltipData;

	protected readonly ItemTooltipData m_CompareItemTooltipData;

	protected readonly bool m_IsScreenWindowTooltip;

	protected bool HasCompareItem => m_CompareItemTooltipData != null;

	public BaseItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
	{
		m_Item = item;
		m_BlueprintItem = item.Blueprint;
		m_ItemTooltipData = itemTooltipData;
		m_CompareItemTooltipData = compareItemTooltipData;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
		ContentSpacing = 0f;
	}

	public BaseItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
	{
		m_BlueprintItem = blueprintItem;
		m_ItemTooltipData = itemTooltipData;
		m_CompareItemTooltipData = compareItemTooltipData;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
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
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade: false, text));
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
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddEndTurnInfo(list, type);
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

	protected void AddIconStatValue(List<ITooltipBrick> bricks, TooltipElement element, Sprite icon = null, TooltipBrickIconStatValueType type = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType bgrType = TooltipBrickIconStatValueType.Normal)
	{
		string text = m_ItemTooltipData.GetText(element);
		string addText = m_ItemTooltipData.GetAddText(element);
		string tooltipElementLabel = UIUtilityTooltip.GetTooltipElementLabel(element);
		AddIconStatValue(bricks, tooltipElementLabel, text, addText, icon, type, bgrType);
	}

	protected bool AddIconStatValue(List<ITooltipBrick> bricks, string label, string value, string addValue, Sprite icon, TooltipBrickIconStatValueType type = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType bgrType = TooltipBrickIconStatValueType.Normal)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		bricks.Add(new TooltipBrickIconStatValue(label, value, addValue, icon, type, bgrType));
		return true;
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
		string value = UIUtilityText.AddPercentTo(m_ItemTooltipData.GetText(TooltipElement.Penetration));
		if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(GetDamageText()))
		{
			StatData damageStatData = GetDamageStatData();
			ComparisonResult comparison = ComparisonResult.Equal;
			if (HasCompareItem)
			{
				CompareData compareData = m_ItemTooltipData.GetCompareData(TooltipElement.Penetration);
				CompareData compareData2 = m_CompareItemTooltipData.GetCompareData(TooltipElement.Penetration);
				comparison = CompareValues(compareData.Value, compareData2.Value);
			}
			StatData.StatHighlight highlight = StatData.StatHighlight.Default;
			if (m_BlueprintItem.PrototypeLink is BlueprintItemWeapon { CanBeUsedInGame: not false } blueprintItemWeapon && m_BlueprintItem is BlueprintItemWeapon blueprintItemWeapon2)
			{
				highlight = ((blueprintItemWeapon2.WarhammerPenetration > blueprintItemWeapon.WarhammerPenetration) ? StatData.StatHighlight.Positive : StatData.StatHighlight.Default);
			}
			StatData rightStat = new StatData(value, UIUtilityTooltip.GetTooltipElementLabel(TooltipElement.Penetration), null, comparison, highlight);
			bricks.Add(new TooltipBrickTwoColumnsStat(damageStatData, rightStat));
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
			highlight = ((blueprintItemWeapon2.DamageMin > blueprintItemWeapon.DamageMin) ? StatData.StatHighlight.Positive : StatData.StatHighlight.Default);
		}
		return new StatData(damageText, UIUtilityTooltip.GetTooltipElementLabel(TooltipElement.Damage), null, comparison, highlight);
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
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
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
				bricks.Add((type == TooltipTemplateType.Info) ? new TooltipBrickSeparator() : new TooltipBrickSeparator(TooltipBrickElementType.Small));
				flag = false;
			}
			if (restrictionData.Inverted)
			{
				if ((type == TooltipTemplateType.Info || restrictionData.RestrictionItems.Any((RestrictionItem item) => !item.MeetPrerequisite)) && !flag2 && !flag4)
				{
					bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
					flag2 = true;
					flag3 = false;
				}
			}
			else if (type == TooltipTemplateType.Info && !flag3 && !flag4)
			{
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.PrerequisiteFeatures, TooltipTitleType.H3));
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
						bricks.Add((type == TooltipTemplateType.Info) ? new TooltipBrickSeparator() : new TooltipBrickSeparator(TooltipBrickElementType.Small));
					}
					else
					{
						int num = ((type == TooltipTemplateType.Info) ? 100 : UIConfig.Instance.SubTextPercentSize);
						bricks.Add(new TooltipBrickItemHeader($"<size={num}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
					}
				}
				if (restrictionItem.UnitFact != null)
				{
					if (restrictionItem.UnitFact is BlueprintFeature feature)
					{
						bricks.Add(new TooltipBrickFeature(feature, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info, null, forceSetName: true));
					}
					else
					{
						bricks.Add(new TooltipBrickFeature(restrictionItem.UnitFact.Name, null, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info));
					}
				}
				else
				{
					TooltipBrickIconStatValueType tooltipBrickIconStatValueType = ((!restrictionItem.MeetPrerequisite) ? TooltipBrickIconStatValueType.Negative : TooltipBrickIconStatValueType.Normal);
					bricks.Add(new TooltipBrickIconStatValue(restrictionItem.Key, restrictionItem.Value, null, null, tooltipBrickIconStatValueType, tooltipBrickIconStatValueType));
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
			bool canEquip = UIUtilityItem.CanEquipItem(m_Item);
			bricks.Add(new TooltipBrickItemRestriction(m_ItemTooltipData.Restrictions, canEquip));
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
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = ability.Name
			};
			TooltipBrickIconPattern.TextFieldValues textFieldValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.Tooltips.CostAP.Text,
				Value = ability.CostAP
			};
			if (!m_IsScreenWindowTooltip)
			{
				TooltipBrickIconPattern.TextFieldValues textFieldValues2 = textFieldValues;
				if (textFieldValues2.TextParams == null)
				{
					textFieldValues2.TextParams = new TextFieldParams();
				}
				textFieldValues.TextParams.FontColor = UIConfig.Instance.TooltipColors.TooltipValue;
			}
			bricks.Add(new TooltipBrickIconPattern(ability.Icon, ability.PatternData, titleValues, textFieldValues, null, tooltip));
			if (!ability.UIProperties.Any())
			{
				continue;
			}
			bricks.Add(new TooltipBricksGroupStart());
			foreach (UIProperty uIProperty in ability.UIProperties)
			{
				bricks.Add(new TooltipBrickIconStatValue(uIProperty.Name, uIProperty.PropertyValue?.ToString() ?? string.Empty, uIProperty.Description));
			}
			bricks.Add(new TooltipBricksGroupEnd());
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
			string name = ((localizedString != null) ? ((string)localizedString) : "");
			TooltipBrickIconStatValueType type = ((bonus.Value > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			bricks.Add(new TooltipBrickIconStatValue(name, UIUtilityText.AddSign(bonus.Value), null, null, type));
		}
	}

	private void AddNotRemovable(List<ITooltipBrick> bricks)
	{
		ItemEntity item = m_Item;
		if (item != null && item.IsNonRemovable)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.IsNotRemovable, TooltipTitleType.H5));
		}
	}

	protected void AddReplenishing(List<ITooltipBrick> bricks)
	{
		string text = m_ItemTooltipData.GetText(TooltipElement.Replenishing);
		if (!string.IsNullOrEmpty(text))
		{
			bricks.Add(new TooltipBrickText(text, TooltipTextType.Italic));
		}
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string text2 = m_ItemTooltipData.GetText(TooltipElement.ShortDescription);
		string text3 = m_ItemTooltipData.GetText(TooltipElement.ArtisticDescription);
		string text4 = m_ItemTooltipData.GetText(TooltipElement.Description) + m_ItemTooltipData.GetText(TooltipElement.LongDescription);
		List<string> additionalDescription = TooltipTemplateUtils.GetAdditionalDescription(m_BlueprintItem);
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			string text6 = text2;
			if (string.IsNullOrEmpty(text6))
			{
				text6 = text4;
				if (string.IsNullOrEmpty(text6))
				{
					text6 = text3;
					if (string.IsNullOrEmpty(text6) && additionalDescription.Count == 0)
					{
						bricks.Add(new TooltipBrickTitle(string.Empty, TooltipTitleType.H6));
						return;
					}
				}
			}
			text6 = TooltipTemplateUtils.AggregateDescription(text6, additionalDescription);
			text6 = TryUpdateWithProperties(text6);
			bricks.Add(new TooltipBrickText(text6, TooltipTextType.Paragraph));
			break;
		}
		case TooltipTemplateType.Info:
			if (!string.IsNullOrEmpty(text3) && !text4.Equals(text3) && !text2.Equals(text3))
			{
				bricks.Add(new TooltipBrickText(text3, TooltipTextType.Italic));
			}
			if (!string.IsNullOrEmpty(text4))
			{
				text4 = TooltipTemplateUtils.AggregateDescription(text4, additionalDescription);
				text4 = TryUpdateWithProperties(text4);
				bricks.Add(new TooltipBrickText(text4, TooltipTextType.Paragraph));
			}
			else if (!string.IsNullOrEmpty(text2))
			{
				text2 = TooltipTemplateUtils.AggregateDescription(text2, additionalDescription);
				text2 = TryUpdateWithProperties(text2);
				bricks.Add(new TooltipBrickText(text2, TooltipTextType.Paragraph));
			}
			else if (additionalDescription.Count > 0)
			{
				string text5 = TooltipTemplateUtils.AggregateDescription("", additionalDescription);
				bricks.Add(new TooltipBrickText(text5, TooltipTextType.Paragraph));
			}
			break;
		}
		ItemEntity item = m_Item;
		if (item != null && item.HasUniqueSourceDescription)
		{
			bricks.Add(new TooltipBrickText(m_Item?.UniqueSourceDescription ?? "", TooltipTextType.Italic));
		}
		string TryUpdateWithProperties(string text)
		{
			return UIUtilityText.UpdateDescriptionWithUIProperties(text, m_Item?.Owner);
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
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			}
			if (!string.IsNullOrEmpty(endTurn))
			{
				bricks.Add(new TooltipBrickIconValueStat(endTurn, null, moveEndPoints, TooltipIconValueStatType.NameTextNormal, isWhite: true, needChangeSize: true, 18, 18, needChangeColor: false, default(Color), default(Color), useSecondaryLabelColor: true));
			}
			if (!string.IsNullOrEmpty(attackAbilityGroupCooldown))
			{
				if (!string.IsNullOrEmpty(endTurn))
				{
					bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
				}
				bricks.Add(new TooltipBrickIconValueStat(attackAbilityGroupCooldown, null, actionEndPoints, TooltipIconValueStatType.NameTextNormal, isWhite: true, needChangeSize: true, 18, 18, needChangeColor: false, default(Color), default(Color), useSecondaryLabelColor: true));
			}
		}
		else
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			if (!string.IsNullOrWhiteSpace(endTurn) || !string.IsNullOrWhiteSpace(attackAbilityGroupCooldown))
			{
				TextFieldParams textFieldParams = new TextFieldParams
				{
					FontColor = UIConfig.Instance.TooltipColors.Default,
					FontStyles = FontStyles.Strikethrough
				};
				list.Add(new TooltipBrickTripleText(endTurn, attackAbilityGroupCooldown, string.Empty, moveEndPoints, actionEndPoints, null, textFieldParams, textFieldParams, textFieldParams));
			}
			if (list.Count > 0)
			{
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
				bricks.AddRange(list);
			}
		}
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
