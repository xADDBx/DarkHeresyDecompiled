using Kingmaker.UI;
using Kingmaker.UI.Pointer;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBackgroundAnimator : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_MainContainer;

	[SerializeField]
	private float m_RotationStrength = 1f;

	[SerializeField]
	private float m_RotationClampMin = -10f;

	[SerializeField]
	private float m_RotationClampMax = 10f;

	[SerializeField]
	private UnitInfoBackgroundParallax[] m_BackgroundParallax;

	public void Animate()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_MainContainer, CursorController.CursorPosition, UICamera.Instance, out var localPoint);
		Vector2 vector = new Vector2(localPoint.x / (m_MainContainer.rect.width * 0.5f), localPoint.y / (m_MainContainer.rect.height * 0.5f));
		UnitInfoBackgroundParallax[] backgroundParallax = m_BackgroundParallax;
		for (int i = 0; i < backgroundParallax.Length; i++)
		{
			backgroundParallax[i].UpdateParallax(vector.x);
		}
		float value = (0f - vector.x) * m_RotationStrength;
		value = Mathf.Clamp(value, m_RotationClampMin, m_RotationClampMax);
		m_MainContainer.localRotation = Quaternion.Lerp(m_MainContainer.localRotation, Quaternion.Euler(0f, value, 0f), Time.deltaTime * 5f);
	}
}
