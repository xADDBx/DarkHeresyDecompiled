using System;
using System.Text;
using Framework.Utility.DotNetExtensions;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("e384f0673f074a0a8a659b52074ba7a7")]
public sealed class CheckCaseStatus : Condition
{
	public enum CheckAnswerType
	{
		None,
		Right,
		Wrong,
		Specific,
		NotSpecific
	}

	[ValidateNotNull]
	public BpRef<BlueprintCase> Case = new BpRef<BlueprintCase>();

	public CaseStatus Status;

	[InfoBox("Проверяет, что дело закрыто без ответа")]
	[ShowIf("IsStatusClosed")]
	public bool IsFailed;

	[ShowIf("CheckClosedWithAnswer")]
	public CheckAnswerType CheckAnswer;

	[ShowIf("CheckSpecificAnswer")]
	public BpRef<BlueprintCaseAnswer> Answer = new BpRef<BlueprintCaseAnswer>();

	private bool IsStatusClosed => Status == CaseStatus.Closed;

	private bool CheckClosedWithAnswer
	{
		get
		{
			if (IsStatusClosed)
			{
				return !IsFailed;
			}
			return false;
		}
	}

	private bool CheckSpecificAnswer
	{
		get
		{
			if (CheckClosedWithAnswer)
			{
				CheckAnswerType checkAnswer = CheckAnswer;
				return checkAnswer == CheckAnswerType.Specific || checkAnswer == CheckAnswerType.NotSpecific;
			}
			return false;
		}
	}

	protected override string GetConditionCaption()
	{
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			value.Append("Case status is ");
			if (IsStatusClosed && IsFailed)
			{
				value.Append("Failed");
			}
			else
			{
				value.Append(Status);
			}
			if (CheckClosedWithAnswer && CheckAnswer != 0)
			{
				value.Append((CheckAnswer == CheckAnswerType.NotSpecific) ? " and answer is not " : " and answer is ");
				value.Append(CheckSpecificAnswer ? Answer : ((object)CheckAnswer));
			}
			return value.ToString();
		}
	}

	protected override bool CheckCondition()
	{
		DetectiveSystem detectiveSystem = Game.Instance.DetectiveSystem;
		if (detectiveSystem.GetCaseStatus(Case) != Status)
		{
			return false;
		}
		if (!IsStatusClosed)
		{
			return true;
		}
		bool hasValue = detectiveSystem.GetCaseAnswer(Case).HasValue;
		if (IsFailed == hasValue)
		{
			return false;
		}
		int result3;
		int result;
		switch (CheckAnswer)
		{
		case CheckAnswerType.None:
			return true;
		case CheckAnswerType.Right:
		{
			(BlueprintCaseQuestion, BlueprintCaseAnswer)? caseAnswer = detectiveSystem.GetCaseAnswer(Case);
			int result4;
			if (caseAnswer.HasValue)
			{
				(BlueprintCaseQuestion, BlueprintCaseAnswer) valueOrDefault2 = caseAnswer.GetValueOrDefault();
				result4 = ((valueOrDefault2.Item1.RightAnswer == valueOrDefault2.Item2) ? 1 : 0);
			}
			else
			{
				result4 = 0;
			}
			return (byte)result4 != 0;
		}
		case CheckAnswerType.Wrong:
		{
			(BlueprintCaseQuestion, BlueprintCaseAnswer)? caseAnswer = detectiveSystem.GetCaseAnswer(Case);
			int result2;
			if (caseAnswer.HasValue)
			{
				(BlueprintCaseQuestion, BlueprintCaseAnswer) valueOrDefault = caseAnswer.GetValueOrDefault();
				result2 = ((valueOrDefault.Item1.RightAnswer != valueOrDefault.Item2) ? 1 : 0);
			}
			else
			{
				result2 = 0;
			}
			return (byte)result2 != 0;
		}
		case CheckAnswerType.Specific:
		{
			(BlueprintCaseQuestion, BlueprintCaseAnswer)? caseAnswer = detectiveSystem.GetCaseAnswer(Case);
			if (caseAnswer.HasValue)
			{
				BlueprintCaseAnswer item2 = caseAnswer.GetValueOrDefault().Item2;
				if (item2 != null)
				{
					result3 = ((Answer == item2) ? 1 : 0);
					goto IL_014b;
				}
			}
			result3 = 0;
			goto IL_014b;
		}
		case CheckAnswerType.NotSpecific:
		{
			(BlueprintCaseQuestion, BlueprintCaseAnswer)? caseAnswer = detectiveSystem.GetCaseAnswer(Case);
			if (caseAnswer.HasValue)
			{
				BlueprintCaseAnswer item = caseAnswer.GetValueOrDefault().Item2;
				if (item != null)
				{
					result = ((Answer != item) ? 1 : 0);
					goto IL_018c;
				}
			}
			result = 0;
			goto IL_018c;
		}
		default:
			{
				throw new ArgumentOutOfRangeException();
			}
			IL_014b:
			return (byte)result3 != 0;
			IL_018c:
			return (byte)result != 0;
		}
	}
}
