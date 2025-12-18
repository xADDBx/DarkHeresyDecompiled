using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class PaperReportEntityVM : ViewModel
{
	public readonly int ClueStartId;

	public readonly int ClueEndId;

	private readonly BlueprintCase m_BlueprintCase;

	private readonly ReportContext m_ReportContext;

	public readonly ReadOnlyReactiveProperty<BlueprintCaseAnswer> SelectedAnswer;

	public PaperReportEntityVM(ReportContext reportContext)
	{
		m_BlueprintCase = reportContext.BlueprintCase;
		m_ReportContext = reportContext;
		SelectedAnswer = m_ReportContext.SelectedAnswer.Select((ReportAnswerVM value) => value?.Answer).ToReadOnlyReactiveProperty().AddTo(this);
		int num = ((reportContext.Answers != null && reportContext.Answers.Count != 0) ? reportContext.Answers.Max((ReportAnswerVM a) => UIUtilityDetective.GetAnswerDegreeDescription(a.Answer).Text.Length) : 0);
		ClueStartId = reportContext.Question.Description.Text.IndexOf("<CLUE>", StringComparison.Ordinal);
		ClueEndId = ClueStartId + num;
		ClueEndId += ((num % 2 != 0) ? 1 : 0);
	}

	public string GetDescription()
	{
		string text = string.Empty;
		for (int i = 0; i < (ClueEndId - ClueStartId) / 2; i++)
		{
			text += "?\u00ad";
		}
		text = "<color=#00000000><mspace=0.5em><color=#00000000>" + text + "?</color></mspace></color>";
		return m_ReportContext.Question.Description.Text.Replace("<CLUE>.", "<CLUE>").Replace("<CLUE>", text);
	}

	public string GetSelectedClueDescription()
	{
		ReportAnswerVM currentValue = m_ReportContext.SelectedAnswer.CurrentValue;
		if (currentValue == null)
		{
			return string.Empty;
		}
		string text = "<color=#FFFFFF00>" + m_ReportContext.Question.Description.Text + "</color>";
		string text2 = UIUtilityDetective.GetAnswerDegreeDescription(currentValue.Answer).Text;
		if (!m_BlueprintCase.IsClosed())
		{
			text2 = text2.Aggregate(string.Empty, (string current, char symbol) => current + $"{symbol}\u00ad");
		}
		return text.Replace("<CLUE>", "</color>" + text2 + "<color=#FFFFFF00>");
	}
}
