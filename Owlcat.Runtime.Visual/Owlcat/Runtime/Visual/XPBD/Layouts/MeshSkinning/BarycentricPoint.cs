using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

[Serializable]
public struct BarycentricPoint
{
	public Vector3 BarycentricCoords;

	public float Height;

	public static BarycentricPoint Zero => new BarycentricPoint(Vector3.zero, 0f);

	public BarycentricPoint(Vector3 position, float height)
	{
		BarycentricCoords = position;
		Height = height;
	}
}
