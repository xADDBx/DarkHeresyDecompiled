using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.DataStructures;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\XPBD\\DataStructures\\InertialFrame.cs")]
public struct InertialForces
{
	public float4 LinearVel;

	public float4 AngularVel;

	public float4 InertialAccel;

	public float4 EulerAccel;
}
