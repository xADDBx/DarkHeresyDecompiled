using DG.Tweening;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class TransformControlsWidget : View<BlueprintMultiEntranceMap>, IScrollHandler, IEventSystemHandler, IDragHandler
{
	[SerializeField]
	private RectTransform m_MapTransform;

	[SerializeField]
	private InventoryRuler m_Ruler;

	[Header("Animation")]
	[SerializeField]
	private float m_Duration = 0.5f;

	[SerializeField]
	private Ease m_Ease = Ease.InOutQuad;

	[Header("Interactive Controls")]
	[SerializeField]
	private float m_MinScale = 0.5f;

	[SerializeField]
	private float m_MaxScale = 2f;

	[SerializeField]
	private float m_ZoomStep = 0.1f;

	[SerializeField]
	private float m_LocalMaxDragDistance = 100f;

	private Tweener m_PositionTween;

	private Tweener m_ScaleTween;

	private Vector2 m_LocalAnchor;

	private bool IsGlobal => base.ViewModel == BlueprintMultiEntranceMap.Global;

	protected override void OnBind()
	{
		m_Ruler.gameObject.SetActive(base.ViewModel == BlueprintMultiEntranceMap.Global);
	}

	public void GoToTarget(RectTransform target)
	{
		float num = Mathf.Max(m_MapTransform.rect.width / (target.rect.width * target.localScale.x), m_MapTransform.rect.height / (target.rect.height * target.localScale.y));
		Animate(m_LocalAnchor = -(Vector2)target.localPosition * num, num);
	}

	public void Reset()
	{
		Animate(Vector2.zero, 1f);
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (IsGlobal)
		{
			KillTweens();
			float num = ((eventData.scrollDelta.y > 0f) ? 1f : (-1f));
			float num2 = Mathf.Clamp(m_MapTransform.localScale.x + num * m_ZoomStep, m_MinScale, m_MaxScale);
			float zoom = (num2 - m_MinScale) / (m_MaxScale - m_MinScale);
			m_Ruler.SetZoom(zoom);
			m_MapTransform.localScale = Vector3.one * num2;
			ClampPosition();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		KillTweens();
		m_MapTransform.anchoredPosition += eventData.delta;
		if (IsGlobal)
		{
			ClampPosition();
		}
		else
		{
			ClampLocalPosition();
		}
	}

	private void ClampPosition()
	{
		RectTransform rectTransform = (RectTransform)m_MapTransform.parent;
		if (!(rectTransform == null))
		{
			float x = m_MapTransform.localScale.x;
			float num = m_MapTransform.rect.width * x;
			float num2 = m_MapTransform.rect.height * x;
			float width = rectTransform.rect.width;
			float height = rectTransform.rect.height;
			float num3 = Mathf.Max(0f, (num - width) * 0.5f);
			float num4 = Mathf.Max(0f, (num2 - height) * 0.5f);
			Vector2 anchoredPosition = m_MapTransform.anchoredPosition;
			anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, 0f - num3, num3);
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, 0f - num4, num4);
			m_MapTransform.anchoredPosition = anchoredPosition;
		}
	}

	private void ClampLocalPosition()
	{
		Vector2 vector = m_MapTransform.anchoredPosition;
		Vector2 vector2 = vector - m_LocalAnchor;
		if (vector2.magnitude > m_LocalMaxDragDistance)
		{
			vector = m_LocalAnchor + vector2.normalized * m_LocalMaxDragDistance;
		}
		m_MapTransform.anchoredPosition = vector;
	}

	private void Animate(Vector2 position, float scale)
	{
		KillTweens();
		if (m_Duration <= 0f)
		{
			m_MapTransform.localPosition = position;
			m_MapTransform.localScale = Vector3.one * scale;
		}
		else
		{
			m_PositionTween = m_MapTransform.DOLocalMove(position, m_Duration).SetEase(m_Ease).SetUpdate(isIndependentUpdate: true);
			m_ScaleTween = m_MapTransform.DOScale(Vector3.one * scale, m_Duration).SetEase(m_Ease).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void OnDisable()
	{
		KillTweens();
		m_MapTransform.anchoredPosition = Vector2.zero;
		m_MapTransform.localScale = Vector3.one;
	}

	private void KillTweens()
	{
		m_PositionTween?.Kill();
		m_ScaleTween?.Kill();
		m_PositionTween = null;
		m_ScaleTween = null;
	}
}
