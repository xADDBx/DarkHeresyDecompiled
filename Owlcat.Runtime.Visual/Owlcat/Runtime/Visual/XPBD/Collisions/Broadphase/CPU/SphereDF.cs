using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct SphereDF : LocalOptimization.IDistanceFunction
{
	public ColliderShape Shape;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorld;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorldPrev;

	public void Evaluate(float4 point, float4 radii, ref LocalOptimization.SurfacePoint projectedPoint)
	{
		float4 @float = Shape.Center * ColliderToWorldPrev.Scale;
		point = ColliderToWorldPrev.InverseTransformPointUnscaled(point) - @float;
		float num = Shape.Size.x * math.cmax(ColliderToWorldPrev.Scale.xyz);
		float num2 = math.length(point);
		float4 float2 = point / (num2 + 1E-06f);
		projectedPoint.Point = ColliderToWorld.TransformPointUnscaled(@float + float2 * (num + Shape.ContactOffset));
		projectedPoint.Normal = ColliderToWorld.TransformDirection(float2);
	}
}
