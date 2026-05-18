using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAnswerConditions : TooltipBaseTemplate
{
	private readonly BlueprintAnswer m_BlueprintAnswer;

	public TooltipTemplateAnswerConditions(BlueprintAnswer blueprintAnswer)
	{
		m_BlueprintAnswer = blueprintAnswer;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(new TextEntity(UIStrings.Instance.Overtips.RequiredResourceCount, TextFieldParams.Left), TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (TryFillSelectConditions(out var result))
		{
			list.AddRange(result);
		}
		return list;
	}

	private bool TryFillSelectConditions(out List<ITooltipBrick> result)
	{
		result = new List<ITooltipBrick>();
		if (m_BlueprintAnswer.SelectConditions.Conditions.Length > 1)
		{
			string text = m_BlueprintAnswer.SelectConditions.Operation switch
			{
				Operation.Or => UIStrings.Instance.Dialog.OperationOrConditionDesc, 
				Operation.And => UIStrings.Instance.Dialog.OperationAndConditionDesc, 
				_ => string.Empty, 
			};
			result.Add(new BrickTextVM(text));
		}
		Condition[] conditions = m_BlueprintAnswer.SelectConditions.Conditions;
		foreach (Condition condition in conditions)
		{
			if (!(condition is ContextConditionHasItem condition2))
			{
				if (!(condition is ItemsEnough condition3))
				{
					if (!(condition is CheckCaseStatus condition4))
					{
						if (condition is HasAlignment alignment)
						{
							AddAlignment(result, alignment);
						}
					}
					else
					{
						AddCaseStatus(result, condition4);
					}
				}
				else
				{
					AddItemsEnough(result, condition3);
				}
			}
			else
			{
				AddRequiredItem(result, condition2);
			}
		}
		return result.Count > 0;
	}

	private static void AddAlignment(List<ITooltipBrick> result, HasAlignment alignment)
	{
		BrickElementPalette backgroundType = (alignment.Check() ? BrickElementPalette.Positive : BrickElementPalette.Negative);
		string formattedCondition = UIUtilityAlignment.GetFormattedCondition(alignment);
		result.Add(new BrickIconStatValueVM(new TextValueAddElement(formattedCondition), null, BrickElementPalette.Normal, backgroundType, null, null, null, null, null, null, hasValue: false));
	}

	private static void AddRequiredItem(List<ITooltipBrick> result, ContextConditionHasItem condition)
	{
		BrickElementPalette backgroundType = (condition.Check() ? BrickElementPalette.Positive : BrickElementPalette.Negative);
		TextValueAddElement info = new TextValueAddElement(condition.ItemToCheck.Name, condition.Quantity.ToString());
		Sprite icon = condition.ItemToCheck.Icon;
		Color? iconColor = Color.white;
		bool hasValue = condition.Quantity > 1;
		result.Add(new BrickIconStatValueVM(info, icon, BrickElementPalette.Normal, backgroundType, null, null, null, iconColor, null, null, hasValue));
	}

	private static void AddCaseStatus(List<ITooltipBrick> result, CheckCaseStatus condition)
	{
		BrickElementPalette backgroundType = (condition.Check() ? BrickElementPalette.Positive : BrickElementPalette.Negative);
		BlueprintCase blueprint = condition.Case.Blueprint;
		string text = blueprint.Name;
		if (condition.Status == CaseStatus.Closed)
		{
			text = string.Format(UIStrings.Instance.Dialog.CaseClosedCondition, text);
		}
		TextValueAddElement info = new TextValueAddElement(text);
		Sprite icon = blueprint.Icon;
		Color? iconColor = Color.white;
		result.Add(new BrickIconStatValueVM(info, icon, BrickElementPalette.Normal, backgroundType, null, null, null, iconColor, null, null, hasValue: false));
	}

	private static void AddItemsEnough(List<ITooltipBrick> result, ItemsEnough condition)
	{
		if (condition.ItemToCheck != null)
		{
			BrickElementPalette backgroundType = (condition.Check() ? BrickElementPalette.Positive : BrickElementPalette.Negative);
			TextValueAddElement info = new TextValueAddElement(condition.ItemToCheck.Name, condition.Quantity.ToString());
			Sprite icon = condition.ItemToCheck.Icon;
			Color? iconColor = Color.white;
			bool hasValue = condition.Quantity > 1;
			result.Add(new BrickIconStatValueVM(info, icon, BrickElementPalette.Normal, backgroundType, null, null, null, iconColor, null, null, hasValue));
		}
	}
}
