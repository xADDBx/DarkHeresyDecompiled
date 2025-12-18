using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

internal class MasterFace
{
	public Vector3 P1;

	public Vector3 P2;

	public Vector3 P3;

	public Vector3 N1;

	public Vector3 N2;

	public Vector3 N3;

	private Vector3 V0;

	private Vector3 V1;

	private float Dot00;

	private float Dot01;

	private float Dot11;

	public Vector3 FaceNormal;

	public float Size;

	public int Index;

	public int3 VertexIndices;

	public uint Master;

	public void CacheBarycentricData()
	{
		V0 = P3 - P1;
		V1 = P2 - P1;
		Dot00 = Vector3.Dot(V0, V0);
		Dot01 = Vector3.Dot(V0, V1);
		Dot11 = Vector3.Dot(V1, V1);
	}

	public bool BarycentricCoords(Vector3 point, ref Vector3 coords)
	{
		Vector3 rhs = point - P1;
		float num = Vector3.Dot(V0, rhs);
		float num2 = Vector3.Dot(V1, rhs);
		float num3 = Dot00 * Dot11 - Dot01 * Dot01;
		if (!Mathf.Approximately(num3, 0f))
		{
			float num4 = (Dot11 * num - Dot01 * num2) / num3;
			float num5 = (Dot00 * num2 - Dot01 * num) / num3;
			coords = new Vector3(1f - num4 - num5, num5, num4);
			return true;
		}
		return false;
	}
}
