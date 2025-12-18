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
					if (!visibleOnly || detectiveSystem.HasItem(caseItem))
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
}
