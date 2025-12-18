using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class Lamp : MonoBehaviour
{
	[Header("Lamp Parts")]
	[SerializeField]
	private Image m_Foreground;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	private RectTransform m_RectTransform;

	public RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
			{
				m_RectTransform = (RectTransform)base.transform;
			}
			return m_RectTransform;
		}
	}

	public void SetColors(Color? foreground, Color? background)
	{
		if ((bool)m_Foreground && foreground.HasValue)
		{
			m_Foreground.color = foreground.Value;
		}
		if ((bool)m_Background && background.HasValue)
		{
			m_Background.color = background.Value;
		}
	}

	public void SetVisible(bool visible)
	{
		EnsureCanvasGroup();
		m_CanvasGroup.alpha = (visible ? 1f : 0f);
		m_CanvasGroup.blocksRaycasts = visible;
		m_CanvasGroup.interactable = visible;
	}

	public void SetAlpha(float alpha)
	{
		EnsureCanvasGroup();
		float num = Mathf.Clamp01(alpha);
		m_CanvasGroup.alpha = num;
		bool flag = num > 0f;
		m_CanvasGroup.blocksRaycasts = flag;
		m_CanvasGroup.interactable = flag;
	}

	private void EnsureCanvasGroup()
	{
		if (m_CanvasGroup == null)
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
			if (m_CanvasGroup == null)
			{
				m_CanvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			}
		}
	}
}
