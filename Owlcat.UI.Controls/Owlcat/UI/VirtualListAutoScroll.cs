using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.UI;

internal class VirtualListAutoScroll : IDisposable
{
	private static readonly Observable<GameObject> s_SelectedGameObject = Observable.Defer(() => (!(EventSystem.current == null)) ? Observable.EveryValueChanged(EventSystem.current, (EventSystem x) => x.currentSelectedGameObject, UnityFrameProvider.PostLateUpdate) : Observable.Return<GameObject>(null)).Replay(1).RefCount();

	private readonly List<VirtualListElement> m_Elements;

	private readonly IInternalScrollController m_Scroll;

	private readonly IDisposable m_Subscription;

	public VirtualListAutoScroll(List<VirtualListElement> elements, IInternalScrollController scroll)
	{
		m_Elements = elements;
		m_Scroll = scroll;
		m_Subscription = s_SelectedGameObject.Subscribe(OnSelectedGameObject);
	}

	private void OnSelectedGameObject(GameObject gameObject)
	{
		VirtualListElement virtualListElement = m_Elements.Find((VirtualListElement e) => e.View is Component component && component.gameObject == gameObject);
		if (virtualListElement != null && !m_Scroll.ElementIsInScrollZone(virtualListElement, out var _))
		{
			m_Scroll.ForceScrollToElement(virtualListElement);
		}
	}

	public void Dispose()
	{
		m_Subscription.Dispose();
	}
}
