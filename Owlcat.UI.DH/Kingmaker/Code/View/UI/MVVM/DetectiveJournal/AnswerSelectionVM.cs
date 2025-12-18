using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswerSelectionVM : SelectionGroupRadioVM<ReportAnswerVM>
{
	public readonly ReportContext ReportContext;

	public AnswerSelectionVM(ReportContext reportContext)
		: base(reportContext.Answers, reportContext.SelectedAnswer, cyclical: false)
	{
		ReportContext = reportContext;
	}
}
