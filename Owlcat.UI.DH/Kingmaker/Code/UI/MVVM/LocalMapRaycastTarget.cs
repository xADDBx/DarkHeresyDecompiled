using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapRaycastTarget : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IScrollHandler, IDragHandler
{
	private Action<PointerEventData> m_OnDrag;

	private Action<PointerEventData> m_OnPointerClick;

	private Action<PointerEventData> m_OnScroll;

	public void Initialize(Action<PointerEventData> onDrag, Action<PointerEventData> onPointerClick, Action<PointerEventData> onScroll)
	{
		m_OnDrag = onDrag;
		m_OnPointerClick = onPointerClick;
		m_OnScroll = onScroll;
	}

	public void OnDrag(PointerEventData eventData)
	{
		m_OnDrag?.Invoke(eventData);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		m_OnPointerClick?.Invoke(eventData);
	}

	public void OnScroll(PointerEventData eventData)
	{
		m_OnScroll?.Invoke(eventData);
	}
}
