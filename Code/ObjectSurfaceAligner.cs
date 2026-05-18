using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class ObjectSurfaceAligner : MonoBehaviour
{
	[Header("Timing")]
	[Min(0.01f)]
	public float interval = 0.5f;

	[Header("Surface")]
	public LayerMask surfaceLayers;

	[Header("Raycast")]
	public float castHeight = 10f;

	public float castDistance = 25f;

	[Header("Offsets")]
	[Tooltip("Offset along averaged surface normal")]
	public float surfaceOffset;

	[Tooltip("Vertical offset (world Y)")]
	public float yOffset;

	[Header("Alignment Constraints")]
	[Tooltip("Maximum allowed slope angle in degrees")]
	[Range(0f, 89f)]
	public float maxSlopeAngle = 45f;

	[Header("Footprint")]
	[Tooltip("Virtual scale of sampling area relative to renderer bounds (1 = exact bounds)")]
	[Min(0.1f)]
	public float footprintScale = 1f;

	[Header("Editor")]
	public bool executeInEditor = true;

	private Coroutine runtimeCoroutine;

	private float lastAlignTime;

	private Vector3 lastPosition;

	private bool hasCachedPosition;

	private Vector3[] gizmoRayOrigins;

	private Vector3[] gizmoHitPoints;

	private bool gizmoSlopeRejected;

	private void OnEnable()
	{
		lastAlignTime = Time.realtimeSinceStartup;
		CachePosition();
		if (Application.isPlaying)
		{
			if (runtimeCoroutine != null)
			{
				StopCoroutine(runtimeCoroutine);
			}
			runtimeCoroutine = StartCoroutine(RuntimeAlignRoutine());
		}
	}

	private void OnDisable()
	{
		if (runtimeCoroutine != null)
		{
			StopCoroutine(runtimeCoroutine);
			runtimeCoroutine = null;
		}
	}

	private IEnumerator RuntimeAlignRoutine()
	{
		while (true)
		{
			TryAlign();
			yield return new WaitForSeconds(interval);
		}
	}

	private void Update()
	{
		if (!Application.isPlaying && executeInEditor)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup - lastAlignTime < interval))
			{
				lastAlignTime = realtimeSinceStartup;
				TryAlign();
			}
		}
	}

	private void TryAlign()
	{
		if (HasPositionChanged())
		{
			AlignToSurface();
			CachePosition();
		}
	}

	private bool HasPositionChanged()
	{
		if (!hasCachedPosition)
		{
			return true;
		}
		return base.transform.position != lastPosition;
	}

	private void CachePosition()
	{
		lastPosition = base.transform.position;
		hasCachedPosition = true;
	}

	private void AlignToSurface()
	{
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		if (componentInChildren == null)
		{
			return;
		}
		Bounds bounds = componentInChildren.bounds;
		Vector3 center = bounds.center;
		Vector3 vector = bounds.extents * footprintScale;
		Vector3[] array = new Vector3[4]
		{
			new Vector3(center.x - vector.x, center.y, center.z - vector.z),
			new Vector3(center.x - vector.x, center.y, center.z + vector.z),
			new Vector3(center.x + vector.x, center.y, center.z - vector.z),
			new Vector3(center.x + vector.x, center.y, center.z + vector.z)
		};
		gizmoRayOrigins = new Vector3[array.Length];
		gizmoHitPoints = new Vector3[array.Length];
		gizmoSlopeRejected = false;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = array[i] + Vector3.up * castHeight;
			gizmoRayOrigins[i] = vector2;
			if (Physics.Raycast(vector2, Vector3.down, out var hitInfo, castDistance, surfaceLayers, QueryTriggerInteraction.Ignore))
			{
				zero += hitInfo.point;
				zero2 += hitInfo.normal;
				gizmoHitPoints[i] = hitInfo.point;
				num++;
			}
			else
			{
				gizmoHitPoints[i] = Vector3.zero;
			}
		}
		if (num != 0)
		{
			zero /= (float)num;
			zero2.Normalize();
			if (Vector3.Angle(zero2, Vector3.up) > maxSlopeAngle)
			{
				gizmoSlopeRejected = true;
				return;
			}
			Vector3 position = zero + zero2 * surfaceOffset + Vector3.up * yOffset;
			base.transform.position = position;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (gizmoRayOrigins == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < gizmoRayOrigins.Length; i++)
		{
			Gizmos.DrawLine(gizmoRayOrigins[i], gizmoRayOrigins[i] + Vector3.down * castDistance);
		}
		for (int j = 0; j < gizmoHitPoints.Length; j++)
		{
			if (!(gizmoHitPoints[j] == Vector3.zero))
			{
				Gizmos.color = (gizmoSlopeRejected ? Color.red : Color.green);
				Gizmos.DrawSphere(gizmoHitPoints[j], 0.08f);
			}
		}
	}
}
