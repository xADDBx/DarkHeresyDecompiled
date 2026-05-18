using System;
using System.Text;
using Framework.Utility.DotNetExtensions;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Utility.Helpers;
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

	[ShowIf("CheckAnyAnswer")]
	public bool AnswerDegree;

	[InfoBox("Нумерация степеней начинается с 0!! Стартовый ансвер - 0")]
	[ShowIf("CheckAnswerDegree")]
	public int AnswerDegreeValue;

	[ShowIf("CheckAnswerDegree")]
	public ComparisionType AnswerDegreeComparision;

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

	private bool CheckAnyAnswer
	{
		get
		{
			if (CheckClosedWithAnswer)
			{
				return CheckAnswer != CheckAnswerType.None;
			}
			return false;
		}
	}

	private bool CheckAnswerDegree
	{
		get
		{
			if (CheckAnyAnswer)
			{
				return AnswerDegree;
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
				if (CheckAnswerDegree)
				{
					value.Append(" and answer degree is ");
					value.Append(AnswerDegreeComparision.GetDescription(AnswerDegreeValue));
				}
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
		(BlueprintCaseQuestion, BlueprintCaseAnswer)? caseAnswer = detectiveSystem.GetCaseAnswer(Case);
		bool hasValue = caseAnswer.HasValue;
		if (IsFailed == hasValue)
		{
			return false;
		}
		bool flag;
		int num3;
		int num;
		switch (CheckAnswer)
		{
		case CheckAnswerType.None:
			flag = true;
			break;
		case CheckAnswerType.Right:
		{
			int num4;
			if (caseAnswer.HasValue)
			{
				(BlueprintCaseQuestion, BlueprintCaseAnswer) valueOrDefault2 = caseAnswer.GetValueOrDefault();
				num4 = ((valueOrDefault2.Item1.RightAnswer == valueOrDefault2.Item2) ? 1 : 0);
			}
			else
			{
				num4 = 0;
			}
			flag = (byte)num4 != 0;
			break;
		}
		case CheckAnswerType.Wrong:
		{
			int num2;
			if (caseAnswer.HasValue)
			{
				(BlueprintCaseQuestion, BlueprintCaseAnswer) valueOrDefault = caseAnswer.GetValueOrDefault();
				num2 = ((valueOrDefault.Item1.RightAnswer != valueOrDefault.Item2) ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
			flag = (byte)num2 != 0;
			break;
		}
		case CheckAnswerType.Specific:
			if (caseAnswer.HasValue)
			{
				BlueprintCaseAnswer item2 = caseAnswer.GetValueOrDefault().Item2;
				if (item2 != null)
				{
					num3 = ((Answer == item2) ? 1 : 0);
					goto IL_0115;
				}
			}
			num3 = 0;
			goto IL_0115;
		case CheckAnswerType.NotSpecific:
			if (caseAnswer.HasValue)
			{
				BlueprintCaseAnswer item = caseAnswer.GetValueOrDefault().Item2;
				if (item != null)
				{
					num = ((Answer != item) ? 1 : 0);
					goto IL_0144;
				}
			}
			num = 0;
			goto IL_0144;
		default:
			{
				throw new ArgumentOutOfRangeException();
			}
			IL_0115:
			flag = (byte)num3 != 0;
			break;
			IL_0144:
			flag = (byte)num != 0;
			break;
		}
		if (!flag)
		{
			return false;
		}
		if (!CheckAnswerDegree)
		{
			return true;
		}
		if (!caseAnswer.HasValue)
		{
			return false;
		}
		if (detectiveSystem.TryGetAnswerDegree(caseAnswer.Value.Item2, out var degree))
		{
			return AnswerDegreeComparision.Check(degree, AnswerDegreeValue);
		}
		return false;
	}
}
