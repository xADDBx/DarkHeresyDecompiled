using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\XPBD\\Bodies\\DeformableVertex.cs")]
public struct DeformableVertex
{
	public float4 PositionBary;

	public float4 NormalBary;

	public float4 TangentBary;

	public int MasterTriangleIndex;

	public int3 ParticleIndices;
}
