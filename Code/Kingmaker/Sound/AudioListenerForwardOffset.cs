using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioListenerForwardOffset : RegisteredBehaviour
{
	[Tooltip("Offset distance along the camera's forward direction. Positive values move the listener further from the camera (deeper into the scene).")]
	[SerializeField]
	private float m_ForwardOffset;

	public float ForwardOffset => m_ForwardOffset;
}
