using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD;

[Serializable]
public class SimulationSettings
{
	public Backend Backend;

	[Range(1f, 16f)]
	public int Substeps = 4;

	[Range(0f, 16f)]
	public int MaxStepsPerFrame = 1;

	public SimulationTickMode TickMode;

	public float SleepThreshold = 0.0005f;

	public float3 Gravity = new float3(0f, -9.8f, 0f);

	public bool CameraCullingEnabled = true;
}
