using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class AerodynamicsConstraintSettings : ConstraintSettings
{
	[Range(0f, 1f)]
	public float Drag = 0.05f;

	[Range(0f, 1f)]
	public float Lift = 0.05f;

	[Min(0f)]
	public float LinearVelocityScale = 1f;

	public override ConstraintType ConstraintType => ConstraintType.Aerodynamics;

	public override float4 GetPackedSettings()
	{
		return new float4(Drag, Lift, LinearVelocityScale, 0f);
	}
}
