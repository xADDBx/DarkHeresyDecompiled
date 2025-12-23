using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\XPBD\\Bodies\\DeformableSkinnedVertex.cs")]
public struct DeformableSkinnedVertex
{
	public float3 Position;

	public float3 Normal;

	public float3 Tangent;

	public float3 _Pad0;
}
