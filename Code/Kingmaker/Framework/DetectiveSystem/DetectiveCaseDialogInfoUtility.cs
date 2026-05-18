using System.Collections.Generic;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Framework.DetectiveSystem;

public static class DetectiveCaseDialogInfoUtility
{
	public static IEnumerable<DialogDetectiveCaseLink> GetRelatedCaseItems(this BlueprintAnswer answer, bool visibleOnly = true)
	{
		return GetRelatedCaseItems(answer.ShowConditions, visibleOnly);
	}

	public static IEnumerable<DialogDetectiveCloseCaseData> GetCloseCaseData(this BlueprintAnswer answer)
	{
		return GetCloseCaseItems(answer.OnSelect);
	}

	private static IEnumerable<DialogDetectiveCaseLink> GetRelatedCaseItems(ConditionsChecker conditions, bool visibleOnly = true)
	{
		DetectiveSystem detectiveSystem = Game.Instance.DetectiveSystem;
		Condition[] conditions2 = conditions.Conditions;
		foreach (Condition condition in conditions2)
		{
			if (condition is IHasDetectiveCaseItemCondition { Not: false } hasDetectiveCaseItemCondition)
			{
				BlueprintCaseItem caseItem = hasDetectiveCaseItemCondition.CaseItem;
				if (caseItem != null)
				{
					if (!visibleOnly || detectiveSystem.HasItemExcludingHidden(caseItem))
					{
						yield return new DialogDetectiveCaseLink(caseItem);
					}
					continue;
				}
			}
			if (condition is CheckCaseStatus checkCaseStatus && !condition.Not && checkCaseStatus.Status == CaseStatus.Opened)
			{
				BpRef<BlueprintCase> @case = checkCaseStatus.Case;
				if ((object)@case != null && detectiveSystem.GetCaseStatus(@case.Blueprint) == CaseStatus.Opened)
				{
					yield return new DialogDetectiveCaseLink(@case.Blueprint);
					continue;
				}
			}
			if (!(condition is OrAndLogic orAndLogic))
			{
				continue;
			}
			foreach (DialogDetectiveCaseLink relatedCaseItem in GetRelatedCaseItems(orAndLogic.ConditionsChecker, visibleOnly))
			{
				yield return relatedCaseItem;
			}
		}
	}

	private static IEnumerable<DialogDetectiveCloseCaseData> GetCloseCaseItems(ActionList onSelect)
	{
		GameAction[] actions = onSelect.Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i] is CloseCase { WithAnswer: not false } closeCase && closeCase.Case != null && closeCase.Answer != null && Game.Instance.DetectiveSystem.GetCaseStatus(closeCase.Case.Blueprint) == CaseStatus.Opened)
			{
				yield return new DialogDetectiveCloseCaseData(closeCase.Case.Blueprint, closeCase.Answer.Blueprint);
			}
		}
	}
}
