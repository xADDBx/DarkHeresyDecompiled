using UnityEngine;

namespace Kingmaker.UI;

[RequireComponent(typeof(Canvas))]
[ExecuteAlways]
public class WorldSpaceCanvasScreenFitter : MonoBehaviour
{
	private const float DefaultOrthoSize = 540f;

	public const float ReferenceWidth = 1920f;

	public const float ReferenceHeight = 1080f;

	private const float ReferenceAspect = 1.7777778f;

	private float m_OrthoSize = 540f;

	private RectTransform m_RectTransform;

	private int m_LastScreenWidth;

	private int m_LastScreenHeight;

	private void OnEnable()
	{
		m_RectTransform = GetComponent<RectTransform>();
		m_OrthoSize = UICamera.Instance?.orthographicSize ?? 540f;
		Apply();
	}

	private void Update()
	{
		if (Screen.width != m_LastScreenWidth || Screen.height != m_LastScreenHeight)
		{
			Apply();
		}
	}

	private void Apply()
	{
		if (!(m_RectTransform == null))
		{
			m_LastScreenWidth = Screen.width;
			m_LastScreenHeight = Screen.height;
			float num = (float)Screen.width / (float)Screen.height;
			float num2;
			float y;
			if (num > 1.7777778f)
			{
				num2 = 1080f * num;
				y = 1080f;
			}
			else
			{
				num2 = 1920f;
				y = 1920f / num;
			}
			m_RectTransform.sizeDelta = new Vector2(num2, y);
			float num3 = m_OrthoSize * 2f * num / num2;
			base.transform.localScale = new Vector3(num3, num3, 1f);
		}
	}
}
