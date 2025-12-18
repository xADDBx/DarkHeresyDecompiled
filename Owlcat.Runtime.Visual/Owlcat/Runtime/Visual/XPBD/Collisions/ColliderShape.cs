using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\XPBD\\Collisions\\ColliderShape.cs", needAccessors = false)]
public struct ColliderShape
{
	public int ShapeType;

	public float4 Center;

	public float4 Size;

	public float ContactOffset;

	public int MaterialIndex;

	public int _Padding;
}
