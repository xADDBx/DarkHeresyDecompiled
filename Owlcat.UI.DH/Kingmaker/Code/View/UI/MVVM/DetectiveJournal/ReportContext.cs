using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ReportContext
{
	public readonly BlueprintCase BlueprintCase;

	public readonly BlueprintCaseQuestion Question;

	public readonly List<ReportAnswerVM> Answers;

	public ReactiveProperty<ReportAnswerVM> SelectedAnswer = new ReactiveProperty<ReportAnswerVM>();

	public ReportContext(BlueprintCase blueprintCase)
	{
		BlueprintCase = blueprintCase;
		Question = Game.Instance.DetectiveSystem.GetActualCaseQuestion(blueprintCase);
		IEnumerable<BlueprintCaseAnswer> answersWithTier = UIUtilityDetective.GetAnswersWithTier(blueprintCase);
		Answers = answersWithTier.Select((BlueprintCaseAnswer a) => new ReportAnswerVM(BlueprintCase, a)).ToList();
		(BlueprintCaseQuestion Question, BlueprintCaseAnswer Answer)? caseAnswer = Game.Instance.DetectiveSystem.GetCaseAnswer(blueprintCase);
		if (caseAnswer.HasValue)
		{
			SelectedAnswer.Value = Answers.FirstOrDefault((ReportAnswerVM avm) => avm.Answer == caseAnswer.Value.Answer);
		}
		else
		{
			BlueprintCaseAnswer prevSelected = UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.GetSelectedAnswer(BlueprintCase);
			SelectedAnswer.Value = Answers.FirstOrDefault((ReportAnswerVM avm) => avm.Answer == prevSelected);
		}
		foreach (BlueprintCaseAnswer item in answersWithTier)
		{
			UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.AddExaminedAnswerIfNeeded(item);
		}
	}
}
