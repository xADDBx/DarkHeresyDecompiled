using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.SceneHelpers;

public sealed class StaticPrefabOcclusionClipGroupBoundsBox : MonoBehaviour
{
	[HideInInspector]
	public Color GizmoColor = new Color(0f, 1f, 1f, 1f);

	public OBox GetBounds()
	{
		OBox oBox = default(OBox);
		oBox.xAxis = new float3(1f, 0f, 0f);
		oBox.yAxis = new float3(0f, 1f, 0f);
		oBox.zAxis = new float3(0f, 0f, 1f);
		oBox.extents = new float3(0.5f);
		float4x4 matrix = base.transform.localToWorldMatrix;
		return oBox.GetTransformedSafe(in matrix);
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = GizmoColor;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.1f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}
}
