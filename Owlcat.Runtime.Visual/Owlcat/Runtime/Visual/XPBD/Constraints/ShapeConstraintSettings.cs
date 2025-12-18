using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class ShapeConstraintSettings : ConstraintSettings
{
	public bool UseSkinNormals;

	[Tooltip("The distance at which the particle should be kept from the skin surface. A negative value means that the particle can penetrate under the skin surface.")]
	public float BackStopDistance;

	public override ConstraintType ConstraintType => ConstraintType.Shape;

	public override float4 GetPackedSettings()
	{
		return new float4(UseSkinNormals ? 1 : 0, BackStopDistance, 0f, 0f);
	}
}
