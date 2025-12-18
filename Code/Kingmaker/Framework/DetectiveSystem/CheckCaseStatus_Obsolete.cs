using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Utility.Helpers;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[Obsolete("New Question/Answer approach, WIP")]
[TypeId("afd13999d7f0459198f3472db5a6a371")]
public sealed class CheckCaseStatus_Obsolete : Condition
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case;

	public CaseStatus Status;

	[Obsolete("New Question/Answer approach, WIP")]
	[ShowIf("IsStatusClosed")]
	public bool CheckCorrectAnswersCount;

	[Obsolete("New Question/Answer approach, WIP")]
	[ShowIf("ShouldCheckCorrectAnswersCount")]
	public ComparisionType CorrectAnswersComparision;

	[Obsolete("New Question/Answer approach, WIP")]
	[ShowIf("ShouldCheckCorrectAnswersCount")]
	public int CorrectAnswersCount;

	private bool IsStatusClosed => Status == CaseStatus.Closed;

	private bool ShouldCheckCorrectAnswersCount
	{
		get
		{
			if (IsStatusClosed)
			{
				return CheckCorrectAnswersCount;
			}
			return false;
		}
	}

	protected override string GetConditionCaption()
	{
		if (!ShouldCheckCorrectAnswersCount)
		{
			return $"Case {Case} has status {Status}";
		}
		return $"Case {Case} has status {Status} and correct answers count is {CorrectAnswersComparision.GetDescription(CorrectAnswersCount)}";
	}

	protected override bool CheckCondition()
	{
		if (Game.Instance.DetectiveSystem.GetCaseStatus(Case) != Status)
		{
			return false;
		}
		if (ShouldCheckCorrectAnswersCount)
		{
			int correctConclusionsCount = Game.Instance.DetectiveSystem.GetCorrectConclusionsCount(Case);
			return CorrectAnswersComparision.Check(correctConclusionsCount, CorrectAnswersCount);
		}
		return true;
	}
}
