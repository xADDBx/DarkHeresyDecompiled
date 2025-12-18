using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct SimplexDF : LocalOptimization.IDistanceFunction
{
	public int Size;

	public FixedArray4<float3> Positions;

	public FixedArray4<float> Radii;

	public CachedTri Tri;

	public void CacheData(in int4 particleIndices, in int size, NativeArray<float3> positions, NativeArray<float> radius)
	{
		Size = size;
		for (int i = 0; i < size; i++)
		{
			Positions[i] = positions[particleIndices[i]];
			Radii[i] = radius[particleIndices[i]];
		}
		if (size == 3)
		{
			Tri.Cache(new float4(Positions[0], 0f), new float4(Positions[1], 0f), new float4(Positions[2], 0f));
		}
	}

	public void Evaluate(float4 point, float4 radii, ref LocalOptimization.SurfacePoint projectedPoint)
	{
		switch (Size)
		{
		default:
		{
			float4 point2 = new float4(Positions[0], 0f);
			projectedPoint.Bary = new float4(1f, 0f, 0f, 0f);
			projectedPoint.Point = point2;
			break;
		}
		case 2:
		{
			float4 a = new float4(Positions[0], 0f);
			float4 b = new float4(Positions[1], 0f);
			XPBDMath.NearestPointOnEdge(in a, in b, in point, out var mu);
			projectedPoint.Bary = new float4(1f - mu, mu, 0f, 0f);
			projectedPoint.Point = a * projectedPoint.Bary[0] + b * projectedPoint.Bary[1];
			break;
		}
		case 3:
			projectedPoint.Point = XPBDMath.NearestPointOnTri(in Tri, point, out projectedPoint.Bary);
			break;
		}
		projectedPoint.Normal = math.normalizesafe(point - projectedPoint.Point);
	}
}
