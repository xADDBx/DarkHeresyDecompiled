using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectivePaperReportView : View<DetectivePaperReportVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private PaperReportDecorView m_DecorView;

	[SerializeField]
	private PaperReportEntityView m_EntityPrefab;

	protected override void OnBind()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Entities, m_EntityPrefab);
		m_DecorView.Bind(base.ViewModel.ReportContext.BlueprintCase);
	}
}
