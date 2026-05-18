using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class ClampedToSphereSegmentPositionProvider : IVector3PositionProvider
{
	private const float AngleThreshold = 160f;

	private readonly Transform m_Main;

	private readonly Transform m_Head;

	private readonly float m_SphereSegmentAngleDeg;

	private readonly float? m_SphereSegmentAngleCos;

	private readonly IVector3PositionProvider m_InternalProvider;

	public Vector3 Position => ClampTargetPositionToSphereSegment(m_InternalProvider.Position);

	public ClampedToSphereSegmentPositionProvider(Transform main, Transform head, float sphereSegmentAngleDeg, IVector3PositionProvider provider)
	{
		m_Main = main;
		m_Head = head;
		m_SphereSegmentAngleDeg = sphereSegmentAngleDeg;
		m_SphereSegmentAngleCos = Mathf.Cos(m_SphereSegmentAngleDeg * (MathF.PI / 180f));
		m_InternalProvider = provider;
	}

	private Vector3 ClampTargetPositionToSphereSegment(Vector3 position)
	{
		Vector3 forward = m_Main.forward;
		Vector3 normalized = (position - m_Head.position).normalized;
		float num = Vector3.Dot(forward, normalized);
		if (num >= m_SphereSegmentAngleCos)
		{
			return position;
		}
		float num2 = Mathf.Acos(num) * 57.29578f;
		float num3 = m_SphereSegmentAngleDeg / num2;
		if (num2 > 160f)
		{
			num3 *= 1f - (num2 - 160f) / 20f;
		}
		return m_Head.position + Vector3.Slerp(forward, normalized, num3);
	}
}
