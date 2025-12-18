using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.AnimatedElements;

public class InventoryRuler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private static readonly int OffsetProperty = Shader.PropertyToID("_Offset");

	private static readonly int ScaleProperty = Shader.PropertyToID("_Scale");

	private static readonly int WidthProperty = Shader.PropertyToID("_Width");

	private static readonly int ShortMarksLengthProperty = Shader.PropertyToID("_ShortMarksLength");

	private static readonly int LongMarksLengthProperty = Shader.PropertyToID("_LongMarksLength");

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private Image[] m_HighlightedImages;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_HighlightColor;

	[SerializeField]
	private Color m_HoveredColor;

	[SerializeField]
	private float m_HighlightDuration;

	[Space]
	[SerializeField]
	private float m_ScaleFrom;

	[SerializeField]
	private float m_ScaleTo;

	[SerializeField]
	private AnimationCurve m_ScaleCurve;

	[Space]
	[SerializeField]
	private float m_WidthFrom;

	[SerializeField]
	private float m_WidthTo;

	[SerializeField]
	private AnimationCurve m_WidthCurve;

	[Space]
	[SerializeField]
	private float m_LongMarksLengthFrom;

	[SerializeField]
	private float m_LongMarksLengthTo;

	[SerializeField]
	private float m_ShortMarksLengthFrom;

	[SerializeField]
	private float m_ShortMarksLengthTo;

	[SerializeField]
	private AnimationCurve m_MarksLengthCurve;

	[Space]
	[SerializeField]
	private float m_CircularLength;

	private Tween m_Tween;

	private Color m_CurrentColor;

	private bool m_IsHovered;

	private Material m_Material;

	private Material Material
	{
		get
		{
			if (!m_Material)
			{
				return m_Material = m_Image.materialForRendering;
			}
			return m_Material;
		}
	}

	public void SetHighlight(bool active)
	{
		if (m_HighlightedImages != null && m_HighlightedImages.Length >= 1)
		{
			m_CurrentColor = (active ? m_HighlightColor : m_NormalColor);
			if (!m_IsHovered)
			{
				SetColor(m_CurrentColor);
			}
		}
	}

	public void SetZoom(float scale)
	{
		float time = Mathf.Clamp01(scale);
		float t = m_ScaleCurve.Evaluate(time);
		float value = Mathf.Lerp(m_ScaleFrom, m_ScaleTo, t);
		float t2 = m_WidthCurve.Evaluate(time);
		float value2 = Mathf.Lerp(m_WidthFrom, m_WidthTo, t2);
		float t3 = m_MarksLengthCurve.Evaluate(time);
		float value3 = Mathf.Lerp(m_ShortMarksLengthFrom, m_ShortMarksLengthTo, t3);
		float value4 = Mathf.Lerp(m_LongMarksLengthFrom, m_LongMarksLengthTo, t3);
		Material.SetFloat(ScaleProperty, value);
		Material.SetFloat(WidthProperty, value2);
		Material.SetFloat(ShortMarksLengthProperty, value3);
		Material.SetFloat(LongMarksLengthProperty, value4);
		m_Image.SetMaterialDirty();
	}

	public void SetRotation(float angle)
	{
		float num = angle / 180f % 1f;
		num = ((angle > 180f) ? (1f - num) : num);
		int num2 = ((!(angle < 180f)) ? 1 : (-1));
		float value = Mathf.Lerp(0f, m_CircularLength, num) * (float)num2;
		Material.SetFloat(OffsetProperty, value);
		m_Image.SetMaterialDirty();
	}

	private void SetColor(Color color)
	{
		m_Tween.Kill();
		Sequence sequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);
		Image[] highlightedImages = m_HighlightedImages;
		foreach (Image target in highlightedImages)
		{
			sequence.Insert(0f, target.DOColor(color, m_HighlightDuration));
		}
		m_Tween = sequence;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		m_IsHovered = true;
		SetColor(m_HoveredColor);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		m_IsHovered = false;
		SetColor(m_CurrentColor);
	}

	private void OnDestroy()
	{
		m_Tween?.Kill();
	}

	private void Awake()
	{
		m_CurrentColor = m_NormalColor;
	}
}
