using UnityEngine;

namespace Kingmaker.UI;

[RequireComponent(typeof(Canvas))]
public class UICameraClaimer : MonoBehaviour
{
	private Canvas m_Canvas;

	[SerializeField]
	private UICamera.UICameraType m_CameraType;

	private void Awake()
	{
		m_Canvas = GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		m_Canvas.worldCamera = UICamera.Claim(m_CameraType);
	}

	private void OnDisable()
	{
		m_Canvas.worldCamera = null;
	}
}
