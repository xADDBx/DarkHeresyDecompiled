using Kingmaker.View;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioListenerPositionController : MonoBehaviour
{
	private DefaultListener m_Listener;

	public bool FreezeXRotation;

	[Header("Ground-Relative Positioning")]
	[Tooltip("When enabled, listener Y position is based on actual ground surface height rather than camera hierarchy. This decouples listener height from CameraPlain objects in the scene.")]
	[SerializeField]
	private bool m_EnableGroundRelativePosition;

	[Tooltip("When enabled, listener XZ is projected to the point where the camera looks at the ground. Compensates for CameraPlain elevation pushing the listener closer to the camera than the visible ground.")]
	[SerializeField]
	private bool m_EnableDepthCompensation;

	[Tooltip("Layer mask for ground raycast. Default: Ground + Water (excludes CameraOnly).")]
	[SerializeField]
	private LayerMask m_GroundRaycastMask = 272;

	[SerializeField]
	private float m_MinHeightAboveGround = 2f;

	[SerializeField]
	private float m_MaxHeightAboveGround = 8f;

	[SerializeField]
	private float m_GroundSmoothSpeed = 8f;

	private Vector3 m_SmoothedPos;

	private bool m_SmoothedPosInitialized;

	private readonly RaycastHit[] m_ReuseHits = new RaycastHit[8];

	public bool UseGroundRelativePosition { get; set; } = true;


	public bool ApplySceneOffset { get; set; } = true;


	private void OnEnable()
	{
		m_Listener = ObjectRegistry<DefaultListener>.Instance?.MaybeSingle;
		if (!m_Listener)
		{
			GameObject gameObject = new GameObject("wWiseListener");
			gameObject.AddComponent<AudioObject>();
			m_Listener = gameObject.AddComponent<DefaultListener>();
			FreezeXRotation = false;
		}
	}

	private void LateUpdate()
	{
		Vector3 vector = base.transform.position;
		Quaternion rotation = (FreezeXRotation ? Quaternion.Euler(78.068f, base.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.z) : base.transform.rotation);
		if (m_EnableGroundRelativePosition && UseGroundRelativePosition && CameraRig.Instance != null)
		{
			vector = GetGroundRelativePosition(vector);
		}
		if (ApplySceneOffset)
		{
			AudioListenerForwardOffset audioListenerForwardOffset = ObjectRegistry<AudioListenerForwardOffset>.Instance?.MaybeSingle;
			if (audioListenerForwardOffset != null && CameraRig.Instance != null)
			{
				vector += CameraRig.Instance.Camera.transform.forward * audioListenerForwardOffset.ForwardOffset;
			}
		}
		m_Listener.transform.SetPositionAndRotation(vector, rotation);
	}

	private Vector3 GetGroundRelativePosition(Vector3 hierarchyPos)
	{
		Vector3 smoothedPos;
		float num;
		if (m_EnableDepthCompensation)
		{
			Transform transform = CameraRig.Instance.Camera.transform;
			if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 500f, m_GroundRaycastMask))
			{
				smoothedPos = hitInfo.point;
				num = hitInfo.point.y;
			}
			else
			{
				num = GetBestGroundY(hierarchyPos);
				smoothedPos = new Vector3(hierarchyPos.x, num, hierarchyPos.z);
			}
		}
		else
		{
			num = GetBestGroundY(hierarchyPos);
			smoothedPos = new Vector3(hierarchyPos.x, num, hierarchyPos.z);
		}
		float t = ((CameraRig.Instance.CameraZoom != null) ? CameraRig.Instance.CameraZoom.CurrentNormalizePosition : 0f);
		float num2 = Mathf.Lerp(m_MaxHeightAboveGround, m_MinHeightAboveGround, t);
		smoothedPos.y = num + num2;
		if (!m_SmoothedPosInitialized)
		{
			m_SmoothedPos = smoothedPos;
			m_SmoothedPosInitialized = true;
		}
		else
		{
			float t2 = Time.unscaledDeltaTime * m_GroundSmoothSpeed;
			m_SmoothedPos.y = Mathf.Lerp(m_SmoothedPos.y, smoothedPos.y, t2);
			if (m_EnableDepthCompensation)
			{
				m_SmoothedPos.x = Mathf.Lerp(m_SmoothedPos.x, smoothedPos.x, t2);
				m_SmoothedPos.z = Mathf.Lerp(m_SmoothedPos.z, smoothedPos.z, t2);
			}
			else
			{
				m_SmoothedPos.x = smoothedPos.x;
				m_SmoothedPos.z = smoothedPos.z;
			}
		}
		return m_SmoothedPos;
	}

	private float GetBestGroundY(Vector3 worldPos)
	{
		int num = Physics.RaycastNonAlloc(new Vector3(worldPos.x, worldPos.y + 100f, worldPos.z), Vector3.down, m_ReuseHits, 200f, m_GroundRaycastMask);
		if (num == 0)
		{
			return worldPos.y;
		}
		float y = CameraRig.Instance.transform.position.y;
		float y2 = m_ReuseHits[0].point.y;
		float num2 = Mathf.Abs(y2 - y);
		for (int i = 1; i < num; i++)
		{
			float num3 = Mathf.Abs(m_ReuseHits[i].point.y - y);
			if (num3 < num2)
			{
				num2 = num3;
				y2 = m_ReuseHits[i].point.y;
			}
		}
		return y2;
	}

	public void PostCinemachineUpdate()
	{
		LateUpdate();
	}
}
