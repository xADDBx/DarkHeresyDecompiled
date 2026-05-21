using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\XPBD\\Bodies\\DeformableVertex.cs")]
public struct DeformableVertex
{
	public float4 PositionBary;

	public float4 NormalBary;

	public float4 TangentBary;

	public int MasterTriangleIndex;

	public int3 ParticleIndices;
}
