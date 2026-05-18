using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBackgroundParallax : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private float m_ParallaxStrength = 1f;

	[SerializeField]
	private float m_ParallaxClampMin = -10f;

	[SerializeField]
	private float m_ParallaxClampMax = 10f;

	private Vector3 m_InitialPosition;

	private void Awake()
	{
		m_InitialPosition = m_RectTransform.anchoredPosition;
	}

	public void UpdateParallax(float offset)
	{
		Vector3 vector = new Vector3(offset * m_ParallaxStrength, 0f, 0f);
		vector.x = Mathf.Clamp(vector.x, m_ParallaxClampMin, m_ParallaxClampMax);
		m_RectTransform.anchoredPosition = Vector3.Lerp((Vector3)m_RectTransform.anchoredPosition, m_InitialPosition + vector, Time.deltaTime * 5f);
	}
}
