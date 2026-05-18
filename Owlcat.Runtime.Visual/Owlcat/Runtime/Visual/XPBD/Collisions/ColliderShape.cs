using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\XPBD\\Collisions\\ColliderShape.cs", needAccessors = false)]
public struct ColliderShape
{
	public int ShapeType;

	public float4 Center;

	public float4 Size;

	public float ContactOffset;

	public int MaterialIndex;

	public int _Padding;
}
