using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[DisallowMultipleComponent]
public class XpbdCollider : XPBDEntity
{
	private Collider m_SourceCollider;

	public ColliderSource Source;

	public ShapeType ShapeType;

	public XpbdColliderParameters Parameters;

	public float ContactOffset;

	public Collider Collider => m_SourceCollider;

	private void OnEnable()
	{
		TryGetComponent<Collider>(out m_SourceCollider);
		XPBD.RegisterCollider(this);
	}

	private void OnDisable()
	{
		XPBD.UnregisterCollider(this);
	}

	internal void UpdateShape(ref ColliderShape shape)
	{
		if (Source == ColliderSource.Unity)
		{
			UpdateUnityShape(ref shape);
		}
		else
		{
			UpdateXpbdShape(ref shape);
		}
		shape.ContactOffset = ContactOffset;
	}

	private void UpdateUnityShape(ref ColliderShape shape)
	{
		if (m_SourceCollider is SphereCollider)
		{
			UpdateUnitySphere(ref shape);
			return;
		}
		if (m_SourceCollider is BoxCollider)
		{
			UpdateUnityBox(ref shape);
			return;
		}
		if (m_SourceCollider is CapsuleCollider)
		{
			UpdateUnityCapsule(ref shape);
			return;
		}
		throw new NotImplementedException();
	}

	private void UpdateXpbdShape(ref ColliderShape shape)
	{
		shape.ShapeType = (int)ShapeType;
		shape.Center = Parameters.Parameters0;
		if (shape.ShapeType == 0)
		{
			shape.Size.xyz = Parameters.Parameters1.x;
		}
		else
		{
			shape.Size = Parameters.Parameters1;
		}
	}

	private void UpdateUnitySphere(ref ColliderShape shape)
	{
		SphereCollider sphereCollider = m_SourceCollider as SphereCollider;
		shape.ShapeType = 0;
		shape.Center.xyz = sphereCollider.center;
		shape.Size.xyz = sphereCollider.radius;
	}

	private void UpdateUnityBox(ref ColliderShape shape)
	{
		BoxCollider boxCollider = m_SourceCollider as BoxCollider;
		shape.ShapeType = 1;
		shape.Center.xyz = boxCollider.center;
		shape.Size.xyz = boxCollider.size;
	}

	private void UpdateUnityCapsule(ref ColliderShape shape)
	{
		CapsuleCollider capsuleCollider = m_SourceCollider as CapsuleCollider;
		shape.ShapeType = 2;
		shape.Center.xyz = capsuleCollider.center;
		shape.Size.x = capsuleCollider.radius;
		shape.Size.y = capsuleCollider.height;
		shape.Size.z = capsuleCollider.direction;
	}

	private void OnDrawGizmosSelected()
	{
	}
}
