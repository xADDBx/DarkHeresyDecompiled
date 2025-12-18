using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class DistanceConstraintSettings : ConstraintSettings
{
	[Range(0f, 1f)]
	public float Compliance;

	[Range(0f, 1f)]
	public float MaxCompression;

	public override ConstraintType ConstraintType => ConstraintType.Distance;

	public DistanceConstraintSettings()
	{
		Enabled = true;
	}

	public override float4 GetPackedSettings()
	{
		return new float4(Compliance, MaxCompression, 0f, 0f);
	}
}
