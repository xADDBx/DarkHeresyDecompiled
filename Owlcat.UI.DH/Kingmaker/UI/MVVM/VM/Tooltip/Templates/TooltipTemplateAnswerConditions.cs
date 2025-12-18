using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Owlcat.UI;
using TMPro;
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
		yield return new TooltipBrickTitle(UIStrings.Instance.Overtips.RequiredResourceCount, TooltipTitleType.H1, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (TryFillSelectConditions(out var result))
		{
			list.AddRange(result);
		}
		else
		{
			FillRequirements(out var result2);
			list.AddRange(result2);
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
			result.Add(new TooltipBrickText(text));
		}
		Condition[] conditions = m_BlueprintAnswer.SelectConditions.Conditions;
		foreach (Condition condition in conditions)
		{
			if (!(condition is ContextConditionHasItem condition2))
			{
				if (!(condition is ItemsEnough condition3))
				{
					if (condition is CheckCaseStatus_Obsolete condition4)
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

	private void FillRequirements(out List<ITooltipBrick> result)
	{
		result = new List<ITooltipBrick>();
		IEnumerable<Requirement> requirements = m_BlueprintAnswer.GetRequirements();
		if (requirements != null && requirements.Any())
		{
			result.Add(new TooltipBrickText(UIStrings.Instance.Dialog.OperationAndConditionDesc, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		}
	}

	private static void AddRequiredItem(List<ITooltipBrick> result, ContextConditionHasItem condition)
	{
		TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		string name = condition.ItemToCheck.Name;
		string value = condition.Quantity.ToString(">=0;<#");
		Sprite icon = condition.ItemToCheck.Icon;
		Color? iconColor = Color.white;
		result.Add(new TooltipBrickIconStatValue(name, value, null, icon, TooltipBrickIconStatValueType.Normal, backgroundType, null, null, null, null, iconColor));
	}

	private static void AddCaseStatus(List<ITooltipBrick> result, CheckCaseStatus_Obsolete condition)
	{
		TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		BlueprintCase blueprint = condition.Case.Blueprint;
		string text = blueprint.Name;
		if (condition.Status == CaseStatus.Closed)
		{
			text = string.Format(UIStrings.Instance.Dialog.CaseClosedCondition, text);
		}
		string name = text;
		Sprite icon = blueprint.Icon;
		Color? iconColor = Color.white;
		result.Add(new TooltipBrickIconStatValue(name, null, null, icon, TooltipBrickIconStatValueType.Normal, backgroundType, null, null, null, null, iconColor, null, null, hasValue: false));
	}

	private static void AddItemsEnough(List<ITooltipBrick> result, ItemsEnough condition)
	{
		if (condition.ItemToCheck != null)
		{
			TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			string name = condition.ItemToCheck.Name;
			Sprite icon = condition.ItemToCheck.Icon;
			Color? iconColor = Color.white;
			result.Add(new TooltipBrickIconStatValue(name, null, null, icon, TooltipBrickIconStatValueType.Normal, backgroundType, null, null, null, null, iconColor, null, null, hasValue: false));
		}
	}
}
