using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
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

	private readonly Subject<Unit> m_StampLanded = new Subject<Unit>();

	private readonly Subject<Unit> m_StampAnimationFinished = new Subject<Unit>();

	public Observable<Unit> StampAnimationFinishedAsObservable => m_StampAnimationFinished;

	protected override void OnBind()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Entities, m_EntityPrefab);
		m_DecorView.Bind(base.ViewModel.ReportContext.BlueprintCase);
	}

	public void OnStampAnimationStart()
	{
		ServiceWindowsSounds.Instance.DetectiveJournal.StampStart.Play(base.gameObject);
	}

	public void OnStampLanded()
	{
		ServiceWindowsSounds.Instance.DetectiveJournal.StampLanded.Play(base.gameObject);
		m_StampLanded.OnNext(Unit.Default);
	}

	public void OnStampAnimationEnd()
	{
		m_StampAnimationFinished.OnNext(Unit.Default);
	}
}
