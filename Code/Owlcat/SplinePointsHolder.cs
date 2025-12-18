using Dreamteck.Splines;
using UnityEngine;

namespace Owlcat;

public class SplinePointsHolder : MonoBehaviour
{
	public SplineComputer m_Spline;

	public Transform m_ControllerTransform;

	public int m_PointIndex;

	public bool m_RelativePosition = true;

	public bool m_UpdatePosition = true;

	private Vector3 m_OriginalPosOffset;

	private void Start()
	{
		SetOffsetPosition();
	}

	public void SetOffsetPosition(Transform origin = null)
	{
		if (origin == null)
		{
			origin = m_ControllerTransform;
		}
		if (m_RelativePosition && origin != null && m_Spline != null)
		{
			m_OriginalPosOffset = origin.position - m_Spline.GetPointPosition(m_PointIndex);
		}
	}

	private void LateUpdate()
	{
		if (!(m_Spline == null) && !(m_ControllerTransform == null) && m_PointIndex >= 0 && m_PointIndex < m_Spline.pointCount && m_UpdatePosition)
		{
			if (m_RelativePosition)
			{
				m_Spline.SetPointPosition(m_PointIndex, m_ControllerTransform.position + m_OriginalPosOffset);
			}
			else
			{
				m_Spline.SetPointPosition(m_PointIndex, m_ControllerTransform.position);
			}
		}
	}
}
