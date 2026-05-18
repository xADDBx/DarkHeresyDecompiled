using System;
using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI;
using Kingmaker.UI.Sound;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CluePointerHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_OwnRectTransform;

	[SerializeField]
	private List<Graphic> m_ClickHandlers = new List<Graphic>();

	[Header("Values")]
	[SerializeField]
	private float m_MaxScreenDeltaToClick = 3f;

	[SerializeField]
	private float m_MoveSoundScaler = 0.01f;

	[SerializeField]
	private bool m_ShowDebugValues;

	private RectTransform m_ParentRectTransform;

	public Action HandleOnPointerEnter;

	public Action HandleOnPointerExit;

	public Action<Vector2> HandleOnPointerUp;

	public Action HandleOnPointerClick;

	private DetectiveJournalClueView m_CurrentClue;

	private Vector2 m_CorrectedPosition;

	private Vector3 m_WorldPositionOnDown;

	private static DetectiveJournalClueView s_DraggingClue;

	private void Awake()
	{
		m_ClickHandlers.ForEach(delegate(Graphic c)
		{
			c.OnPointerDownAsObservable().Subscribe(OnPointerDown).AddTo(this);
			c.OnDragAsObservable().Subscribe(OnDrag).AddTo(this);
			c.OnPointerUpAsObservable().Subscribe(OnPointerUp).AddTo(this);
		});
	}

	public void SetCaseContext(DetectiveJournalClueView currentClue, RectTransform cluesContainer)
	{
		m_CurrentClue = currentClue;
		m_ParentRectTransform = cluesContainer;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		m_CorrectedPosition = eventData.position - (Vector2)UICamera.Instance.WorldToScreenPoint(m_OwnRectTransform.position);
		m_OwnRectTransform.SetAsLastSibling();
		m_WorldPositionOnDown = m_OwnRectTransform.position;
		if (eventData.button == PointerEventData.InputButton.Left && !(eventData.pointerPressRaycast.gameObject != eventData.pointerCurrentRaycast.gameObject))
		{
			s_DraggingClue = m_CurrentClue;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		UISounds.Instance.Play(ServiceWindowsSounds.Instance.DetectiveJournal.CaseItemDragStart, base.gameObject);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData != null)
		{
			Vector2 screenPoint = eventData.position - m_CorrectedPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, screenPoint, UICamera.Instance, out var localPoint);
			Vector2 anchoredPosition = UIUtilityRect.LimitPositionRectInRect(localPoint, m_ParentRectTransform, m_OwnRectTransform);
			float value = m_ParentRectTransform.InverseTransformVector(eventData.delta).sqrMagnitude * m_MoveSoundScaler;
			value = Mathf.Clamp(value, 0f, 100f);
			AkUnitySoundEngine.SetRTPCValue("DetectiveJournalMoveSmall", value, base.gameObject);
			if (m_ShowDebugValues)
			{
				Debug.Log($"sound RTPC {value}");
			}
			m_OwnRectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		UISounds.Instance.Play(ServiceWindowsSounds.Instance.DetectiveJournal.CaseItemDragStop, base.gameObject);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		HandleOnPointerUp?.Invoke(m_OwnRectTransform.anchoredPosition);
		if (Time.unscaledTime - eventData.clickTime < 0.25f && (m_WorldPositionOnDown - m_OwnRectTransform.position).magnitude < m_MaxScreenDeltaToClick)
		{
			HandleClick();
		}
		s_DraggingClue = null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		HandleOnPointerEnter?.Invoke();
		if (!(s_DraggingClue != null))
		{
			m_OwnRectTransform.SetAsLastSibling();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HandleOnPointerExit?.Invoke();
	}

	private void HandleClick()
	{
		HandleOnPointerClick?.Invoke();
	}
}
