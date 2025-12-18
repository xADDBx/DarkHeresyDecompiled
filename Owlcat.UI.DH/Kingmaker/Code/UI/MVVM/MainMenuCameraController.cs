using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuCameraController : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private Transform m_CameraTransform;

	public void SetCameraPosition(Transform anchor)
	{
		m_CameraTransform.SetPositionAndRotation(anchor.position, anchor.rotation);
	}
}
