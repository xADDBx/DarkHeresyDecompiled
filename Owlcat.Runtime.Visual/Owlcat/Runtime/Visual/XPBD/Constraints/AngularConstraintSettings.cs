using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public class AngularConstraintSettings : ConstraintSettings
{
	[Range(0f, 1f)]
	public float Stiffness;

	[Range(0f, 1f)]
	public float VelocityAttenuation;

	public bool UseParentSpace;

	public override ConstraintType ConstraintType => ConstraintType.Angular;

	public override float4 GetPackedSettings()
	{
		return new float4(Stiffness, VelocityAttenuation, UseParentSpace ? 1 : 0, 0f);
	}
}
