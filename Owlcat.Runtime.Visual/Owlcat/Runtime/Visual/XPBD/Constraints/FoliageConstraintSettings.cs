using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class FoliageConstraintSettings : ConstraintSettings
{
	[Range(0f, 1f)]
	public float Stiffness = 1f;

	[Range(0f, 1f)]
	public float VelocityAttenuation = 0.001f;

	public bool UseParentSpace;

	public override ConstraintType ConstraintType => ConstraintType.Foliage;

	public FoliageConstraintSettings()
	{
		Enabled = false;
	}

	public override float4 GetPackedSettings()
	{
		return new float4(Stiffness, VelocityAttenuation, UseParentSpace ? 1 : 0, 0f);
	}
}
