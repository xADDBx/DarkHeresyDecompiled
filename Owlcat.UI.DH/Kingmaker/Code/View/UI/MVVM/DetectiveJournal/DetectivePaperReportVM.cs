using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectivePaperReportVM : ViewModel
{
	public readonly ReportContext ReportContext;

	public readonly List<PaperReportEntityVM> Entities;

	public DetectivePaperReportVM(ReportContext reportContext)
	{
		ReportContext = reportContext;
		Entities = new List<PaperReportEntityVM>
		{
			new PaperReportEntityVM(reportContext)
		};
	}
}
