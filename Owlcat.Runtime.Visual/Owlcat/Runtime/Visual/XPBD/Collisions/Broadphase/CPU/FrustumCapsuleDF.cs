using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct FrustumCapsuleDF : LocalOptimization.IDistanceFunction
{
	public ColliderShape Shape;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorld;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorldPrev;

	public void Evaluate(float4 point, float4 radii, ref LocalOptimization.SurfacePoint projectedPoint)
	{
		float3 xyz = Shape.Center.xyz * ColliderToWorldPrev.Scale.xyz;
		float3 xyz2 = Shape.Size.xyz * ColliderToWorldPrev.Scale.xyz;
		float start = Shape.Center.w * math.cmax(ColliderToWorldPrev.Scale.xyz);
		float end = Shape.Size.w * math.cmax(ColliderToWorldPrev.Scale.xyz);
		point = ColliderToWorldPrev.InverseTransformPointUnscaled(point);
		float4 a = new float4(xyz, 0f);
		float4 b = new float4(xyz2, 0f);
		float mu;
		float4 @float = XPBDMath.NearestPointOnEdge(in a, in b, in point, out mu);
		float4 float2 = point - @float;
		float num = math.length(float2);
		float4 float3 = float2 / (num + 1E-06f);
		float num2 = math.lerp(start, end, mu);
		projectedPoint.Point = ColliderToWorld.TransformPointUnscaled(@float + float3 * (num2 + Shape.ContactOffset));
		projectedPoint.Normal = ColliderToWorld.TransformDirection(float3);
	}
}
