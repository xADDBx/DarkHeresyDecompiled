using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Items;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.UI;
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
				answer.OnSelect.AssembleActionsOfType(list2);
				foreach (AddItemToPlayer item in list2)
				{
					BlueprintItem itemToGive = item.ItemToGive;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToGive.CreateEntity());
						list.Add(new BrickExchangeVM(new TextValueAddElement(itemToGive.Name, item.Quantity.ToString()), itemToGive.ItemType.ToString(), itemToGive.Icon, BrickElementPalette.Positive, BrickElementPalette.Positive, tooltip, Color.white));
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
				answer.OnSelect.AssembleActionsOfType(list2);
				foreach (RemoveItemFromPlayer item in list2)
				{
					BlueprintItem itemToRemove = item.ItemToRemove;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToRemove.CreateEntity());
						list.Add(new BrickExchangeVM(new TextValueAddElement(itemToRemove.Name, item.Quantity.ToString()), itemToRemove.ItemType.ToString(), itemToRemove.Icon, BrickElementPalette.Negative, BrickElementPalette.Negative, tooltip, Color.white));
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
				List<AddReputation> list2 = new List<AddReputation>();
				answer.OnSelect.AssembleActionsOfType(list2);
				foreach (AddReputation item in list2)
				{
					int value = item.Value.GetValue();
					if ((value > 0 && isPositive) || (value < 0 && !isPositive))
					{
						BrickElementPalette type = (isPositive ? BrickElementPalette.Positive : BrickElementPalette.Negative);
						string factionLabel = UIStrings.Instance.CharacterSheet.GetFactionLabel(item.FactionType);
						TooltipTemplateSimple tooltipTemplateSimple = new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(item.FactionType), UIStrings.Instance.CharacterSheet.GetFactionDescription(item.FactionType));
						TextValueAddElement info = new TextValueAddElement(factionLabel, value.ToString());
						Color? iconColor = null;
						TooltipBaseTemplate tooltip = tooltipTemplateSimple;
						list.Add(new BrickIconStatValueVM(info, null, type, BrickElementPalette.Normal, tooltip, null, null, iconColor));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
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
			list.Add(new BrickSeparatorVM(TooltipBrickElementType.Medium));
			list.Add(new BrickTitleVM(new TextEntity(UIStrings.Instance.Tooltips.YouWillGainTitle.Text, TextFieldParams.Left), TooltipTitleType.H1));
			list.AddRange(gainBricks);
		}
		if (looseBricks.Any())
		{
			list.Add(new BrickSeparatorVM(TooltipBrickElementType.Medium));
			list.Add(new BrickTitleVM(new TextEntity(UIStrings.Instance.Tooltips.YouWillLoseTitle.Text, TextFieldParams.Left), TooltipTitleType.H1));
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
	}
}
