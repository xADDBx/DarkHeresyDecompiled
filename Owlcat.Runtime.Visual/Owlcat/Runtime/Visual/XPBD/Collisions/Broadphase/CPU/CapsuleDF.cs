using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct CapsuleDF : LocalOptimization.IDistanceFunction
{
	public ColliderShape Shape;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorld;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorldPrev;

	public void Evaluate(float4 point, float4 radii, ref LocalOptimization.SurfacePoint projectedPoint)
	{
		float4 @float = Shape.Center * ColliderToWorldPrev.Scale;
		point = ColliderToWorldPrev.InverseTransformPointUnscaled(point) - @float;
		int num = (int)Shape.Size.z;
		float num2 = Shape.Size.x * math.max(ColliderToWorldPrev.Scale[(num + 1) % 3], ColliderToWorldPrev.Scale[(num + 2) % 3]);
		float num3 = math.max(num2, Shape.Size.y * 0.5f * ColliderToWorldPrev.Scale[num]);
		float4 b = float4.zero;
		b[num] = num3 - num2;
		float4 a = -b;
		float mu;
		float4 float2 = XPBDMath.NearestPointOnEdge(in a, in b, in point, out mu);
		float4 float3 = point - float2;
		float num4 = math.length(float3);
		float4 float4 = float3 / (num4 + 1E-06f);
		projectedPoint.Point = ColliderToWorld.TransformPointUnscaled(@float + float2 + float4 * (num2 + Shape.ContactOffset));
		projectedPoint.Normal = ColliderToWorld.TransformDirection(float4);
	}
}
