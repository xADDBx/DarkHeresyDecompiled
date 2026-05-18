using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DeductionPointerHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerEnterHandler
{
	[Header("Values")]
	[SerializeField]
	private float m_MaxScreenDeltaToClick = 4f;

	[SerializeField]
	private float m_MoveSoundScaler = 0.01f;

	public Action HandleOnPointerClick;

	private Vector2 m_RelativePivot = new Vector2(0.5f, 0.5f);

	private DeductionOnScreenView m_ParentView;

	private RectTransform m_ParentRectTransform;

	private ILineTarget m_CaseItemFrom;

	private ILineTarget m_CaseItemTo;

	private bool m_IsDragging;

	private Vector3 m_WorldPositionOnDown;

	public void Initialize(DeductionOnScreenView view, ILineTarget caseItemFrom, ILineTarget caseItemTo, RectTransform cluesParent)
	{
		m_ParentView = view;
		m_CaseItemFrom = caseItemFrom;
		m_CaseItemTo = caseItemTo;
		m_ParentRectTransform = cluesParent;
		UpdatePosition();
	}

	public void SetupPosition(Vector2 position)
	{
		m_ParentView.RectTransform.anchoredPosition = position;
		UpdatePivot();
	}

	private void UpdatePosition()
	{
		if (!m_IsDragging && m_CaseItemFrom != null && m_CaseItemTo != null)
		{
			Vector3 position = m_CaseItemFrom.RectTransform.position;
			Vector3 vector = m_CaseItemTo.RectTransform.position - position;
			Vector3 position2 = m_ParentView.RectTransform.position;
			float x = m_RelativePivot.x;
			if (x >= 0f && x <= 1f)
			{
				position2.x = position.x + vector.x * m_RelativePivot.x;
			}
			x = m_RelativePivot.y;
			if (x >= 0f && x <= 1f)
			{
				position2.y = position.y + vector.y * m_RelativePivot.y;
			}
			if (m_ParentView.RectTransform.position != position2)
			{
				m_ParentView.RectTransform.position = position2;
				UpdatePivot();
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		m_IsDragging = true;
		m_ParentView.transform.SetAsLastSibling();
		m_WorldPositionOnDown = m_ParentView.transform.position;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		ServiceWindowsSounds.Instance.DetectiveJournal.CaseItemDragStart.Play();
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransform rectTransform = MainCanvas.Instance.RectTransform;
		float num = rectTransform.sizeDelta.x / (float)Screen.width * (rectTransform.localScale.x / m_ParentRectTransform.lossyScale.x);
		Vector2 nPos = m_ParentView.RectTransform.anchoredPosition + eventData.delta * num;
		nPos = UIUtilityRect.LimitPositionRectInRect(nPos, m_ParentRectTransform, m_ParentView.RectTransform);
		float value = m_ParentRectTransform.InverseTransformVector(eventData.delta).sqrMagnitude * m_MoveSoundScaler;
		value = Mathf.Clamp(value, 0f, 100f);
		AkUnitySoundEngine.SetRTPCValue("DetectiveJournalMoveSmall", value, base.gameObject);
		m_ParentView.RectTransform.anchoredPosition = nPos;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		ServiceWindowsSounds.Instance.DetectiveJournal.CaseItemDragStop.Play();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (Time.unscaledTime - eventData.clickTime < 0.25f && (m_WorldPositionOnDown - m_ParentView.transform.position).magnitude < m_MaxScreenDeltaToClick)
		{
			HandleOnPointerClick?.Invoke();
		}
		m_ParentView.SaveSharedPosition(m_ParentView.RectTransform.anchoredPosition);
		UpdatePivot();
		UpdatePosition();
		m_IsDragging = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!m_IsDragging)
		{
			m_ParentView.transform.SetAsLastSibling();
		}
	}

	private void UpdatePivot()
	{
		if (m_CaseItemTo != null && m_CaseItemFrom != null)
		{
			Vector2 relativePivot = m_ParentView.RectTransform.anchoredPosition - m_CaseItemFrom.RectTransform.anchoredPosition;
			Vector2 vector = m_CaseItemTo.RectTransform.anchoredPosition - m_CaseItemFrom.RectTransform.anchoredPosition;
			relativePivot.x /= vector.x;
			relativePivot.y /= vector.y;
			m_RelativePivot = relativePivot;
		}
	}
}
