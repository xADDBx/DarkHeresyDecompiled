using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

[Serializable]
public struct SkinTransform
{
	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Scale;

	public SkinTransform(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	public SkinTransform(Transform transform)
	{
		Position = transform.position;
		Rotation = transform.rotation;
		Scale = transform.localScale;
	}

	public void Apply(Transform transform)
	{
		transform.position = Position;
		transform.rotation = Rotation;
		transform.localScale = Scale;
	}

	public Matrix4x4 GetMatrix4X4()
	{
		return Matrix4x4.TRS(Position, Rotation, Scale);
	}

	public void Reset()
	{
		Position = Vector3.zero;
		Rotation = Quaternion.identity;
		Scale = Vector3.one;
	}
}
