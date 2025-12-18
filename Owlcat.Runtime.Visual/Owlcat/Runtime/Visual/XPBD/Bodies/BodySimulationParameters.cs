using System;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[Serializable]
public class BodySimulationParameters
{
	[Range(0f, 1f)]
	public float Damping;

	[Range(0f, 1f)]
	public float LinearInertiaScale = 1f;

	[Range(0f, 1f)]
	public float AngularInertiaScale = 1f;

	[Min(0f)]
	public float TeleportDistanceThreshold;

	public bool DisableWind;

	public static BodySimulationParameters Default => new BodySimulationParameters
	{
		Damping = 0f,
		LinearInertiaScale = 1f,
		AngularInertiaScale = 1f,
		TeleportDistanceThreshold = 0f,
		DisableWind = false
	};

	public float4 GetPackedParameters()
	{
		return new float4(XPBDMath.PackWindFlagAndDamping(!DisableWind, Damping), LinearInertiaScale, AngularInertiaScale, TeleportDistanceThreshold);
	}

	internal int CalculateHash()
	{
		return HashCode.Combine(Damping, LinearInertiaScale, AngularInertiaScale, TeleportDistanceThreshold, DisableWind);
	}
}
