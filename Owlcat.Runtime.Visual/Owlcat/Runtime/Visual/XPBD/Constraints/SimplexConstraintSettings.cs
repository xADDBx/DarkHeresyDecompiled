using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class SimplexConstraintSettings : ConstraintSettings
{
	public ParticleCollisionMode ParticleCollisionMode;

	[IntLayerMask]
	public int CollisionMask;

	[Range(0f, 1f)]
	public float Friction;

	public override ConstraintType ConstraintType => ConstraintType.Simplex;

	public override float4 GetPackedSettings()
	{
		return new float4(math.asfloat(CollisionMask), Friction, (float)ParticleCollisionMode, 0f);
	}
}
