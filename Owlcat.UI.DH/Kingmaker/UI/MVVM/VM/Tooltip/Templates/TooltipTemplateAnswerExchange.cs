using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Items;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAnswerExchange : TooltipBaseTemplate
{
	private static class OnSelectConversionUtils
	{
		public static List<ITooltipBrick> GetGainItemsBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<AddItemToPlayer> list2 = new List<AddItemToPlayer>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (AddItemToPlayer item in list2)
				{
					BlueprintItem itemToGive = item.ItemToGive;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToGive.CreateEntity());
						list.Add(new TooltipBrickExchange(itemToGive.Name, item.Quantity, null, itemToGive.ItemType.ToString(), itemToGive.Icon, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, Color.white, null, null, hasValue: false));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetLooseItemsBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<RemoveItemFromPlayer> list2 = new List<RemoveItemFromPlayer>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (RemoveItemFromPlayer item in list2)
				{
					BlueprintItem itemToRemove = item.ItemToRemove;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToRemove.CreateEntity());
						list.Add(new TooltipBrickExchange(itemToRemove.Name, item.Quantity, null, itemToRemove.ItemType.ToString(), itemToRemove.Icon, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, Color.white, null, null, hasValue: false));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect RemoveItemFromPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetGainFactionReputationBricks(BlueprintAnswer answer, bool isPositive)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<GainFactionReputation> list2 = new List<GainFactionReputation>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (GainFactionReputation item in list2)
				{
					if ((item.Reputation > 0 && isPositive) || (item.Reputation < 0 && !isPositive))
					{
						TooltipBrickIconStatValueType type = (isPositive ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
						string factionLabel = UIStrings.Instance.CharacterSheet.GetFactionLabel(item.Faction);
						Sprite icon = null;
						TooltipTemplateSimple tooltipTemplateSimple = new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(item.Faction), UIStrings.Instance.CharacterSheet.GetFactionDescription(item.Faction));
						string value = item.Reputation.ToString();
						Color? iconColor = null;
						TooltipBaseTemplate tooltip = tooltipTemplateSimple;
						list.Add(new TooltipBrickIconStatValue(factionLabel, value, null, icon, type, TooltipBrickIconStatValueType.Normal, null, tooltip, null, null, iconColor));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetGainColonyResourcesBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<GainColonyResources> list2 = new List<GainColonyResources>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (GainColonyResources item in list2)
				{
					ResourceData[] resources = item.Resources;
					foreach (ResourceData resourceData in resources)
					{
						BlueprintResource blueprintResource = resourceData?.Resource?.Get();
						if (blueprintResource != null)
						{
							TooltipTemplateSimple tooltip = new TooltipTemplateSimple(blueprintResource.Name, blueprintResource.Description);
							list.Add(new TooltipBrickIconStatValue(blueprintResource.Name, resourceData.Count.ToString(), null, blueprintResource.Icon, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Normal, null, tooltip));
						}
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetLooseColonyResourcesBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<RemoveColonyResources> list2 = new List<RemoveColonyResources>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (RemoveColonyResources item in list2)
				{
					ResourceData[] resources = item.Resources;
					foreach (ResourceData resourceData in resources)
					{
						BlueprintResource blueprintResource = resourceData?.Resource?.Get();
						if (blueprintResource != null)
						{
							TooltipTemplateSimple tooltip = new TooltipTemplateSimple(blueprintResource.Name, blueprintResource.Description);
							list.Add(new TooltipBrickIconStatValue(blueprintResource.Name, resourceData.Count.ToString(), null, blueprintResource.Icon, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueType.Normal, null, tooltip));
						}
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		private static void AssembleActionsOfType<T>(GameAction[] actions, List<T> result) where T : GameAction
		{
			if (result == null)
			{
				result = new List<T>();
			}
			result.AddRange(actions.OfType<T>().ToList());
			foreach (Conditional item in actions.OfType<Conditional>())
			{
				AssembleActionsOfType((item.ConditionsChecker.Check() ? item.IfTrue : item.IfFalse).Actions, result);
			}
		}
	}

	private readonly BlueprintAnswer m_BlueprintAnswer;

	public TooltipTemplateAnswerExchange(BlueprintAnswer blueprintAnswer)
	{
		m_BlueprintAnswer = blueprintAnswer;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		GetRewardsOnSelect(out var gainBricks, out var looseBricks);
		if (gainBricks.Any())
		{
			list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.YouWillGainTitle.Text, TooltipTitleType.H1, TextAlignmentOptions.Left));
			list.AddRange(gainBricks);
		}
		if (looseBricks.Any())
		{
			list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.YouWillLoseTitle.Text, TooltipTitleType.H1, TextAlignmentOptions.Left));
			list.AddRange(looseBricks);
		}
		return list;
	}

	private void GetRewardsOnSelect(out List<ITooltipBrick> gainBricks, out List<ITooltipBrick> looseBricks)
	{
		gainBricks = new List<ITooltipBrick>();
		looseBricks = new List<ITooltipBrick>();
		gainBricks.AddRange(OnSelectConversionUtils.GetGainItemsBricks(m_BlueprintAnswer));
		looseBricks.AddRange(OnSelectConversionUtils.GetLooseItemsBricks(m_BlueprintAnswer));
		gainBricks.AddRange(OnSelectConversionUtils.GetGainFactionReputationBricks(m_BlueprintAnswer, isPositive: true));
		looseBricks.AddRange(OnSelectConversionUtils.GetGainFactionReputationBricks(m_BlueprintAnswer, isPositive: false));
		gainBricks.AddRange(OnSelectConversionUtils.GetGainColonyResourcesBricks(m_BlueprintAnswer));
		looseBricks.AddRange(OnSelectConversionUtils.GetLooseColonyResourcesBricks(m_BlueprintAnswer));
	}
}
