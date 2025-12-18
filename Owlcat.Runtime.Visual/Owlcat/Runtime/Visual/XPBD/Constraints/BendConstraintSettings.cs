using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class BendConstraintSettings : ConstraintSettings
{
	[Range(0f, 1f)]
	public float Compliance;

	[Range(0f, 1f)]
	public float MaxBending = 0.025f;

	public override ConstraintType ConstraintType => ConstraintType.Bend;

	public override float4 GetPackedSettings()
	{
		return new float4(Compliance, MaxBending, 0f, 0f);
	}
}
