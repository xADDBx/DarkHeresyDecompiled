using Dreamteck.Splines;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat;

public class SplinePointsBoneBinder : MonoBehaviour
{
	public SplineComputer m_Spline;

	public int m_PointIndex;

	public Skeleton.BoneType m_Bone;

	public Vector3 m_PosOffset;

	public Quaternion m_RotOffset;

	[Tooltip("Update position on Attach")]
	public bool m_Init = true;

	[Tooltip("Keep Updating position")]
	public bool m_Update = true;

	private Transform m_BoneTransform;

	public void Attach(Transform skeletonRoot)
	{
		if (!(m_Bone.BoneName == string.Empty))
		{
			m_BoneTransform = skeletonRoot.FindChildRecursive(m_Bone.BoneName);
			if (m_Init)
			{
				UpdatePosition();
			}
		}
	}

	public void AttachToTransform(Transform target)
	{
		m_BoneTransform = target;
		if (m_Init)
		{
			UpdatePosition();
		}
	}

	public void Update()
	{
		if (m_Update)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		if (!(m_Spline == null) && !(m_BoneTransform == null) && m_PointIndex >= 0 && m_PointIndex < m_Spline.pointCount)
		{
			Vector3 position = m_BoneTransform.position;
			Vector3 vector = m_BoneTransform.TransformDirection(m_PosOffset);
			m_Spline.SetPointPosition(m_PointIndex, position + vector);
		}
	}
}
