using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[DisallowMultipleComponent]
public class ObservableScrollTrigger : ObservableTriggerBase, IScrollHandler, IEventSystemHandler
{
	private Subject<PointerEventData> m_OnScroll;

	protected override void RaiseOnCompletedOnDestroy()
	{
		if (m_OnScroll != null)
		{
			m_OnScroll.OnCompleted();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		m_OnScroll?.OnNext(eventData);
	}

	public Observable<PointerEventData> OnScrollAsObservable()
	{
		return m_OnScroll ?? (m_OnScroll = new Subject<PointerEventData>());
	}
}
