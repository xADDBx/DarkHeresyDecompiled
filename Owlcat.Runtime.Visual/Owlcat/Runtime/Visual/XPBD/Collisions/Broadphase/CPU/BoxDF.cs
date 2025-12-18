using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct BoxDF : LocalOptimization.IDistanceFunction
{
	public ColliderShape Shape;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorld;

	public Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform ColliderToWorldPrev;

	public void Evaluate(float4 point, float4 radii, ref LocalOptimization.SurfacePoint projectedPoint)
	{
		float4 @float = Shape.Center * ColliderToWorldPrev.Scale;
		float4 float2 = Shape.Size * ColliderToWorldPrev.Scale * 0.5f;
		point = ColliderToWorldPrev.InverseTransformPointUnscaled(point) - @float;
		float4 float3 = float2 - math.abs(point);
		if (float3.x >= 0f && float3.y >= 0f && float3.z >= 0f)
		{
			float num = float.MaxValue;
			int index = 0;
			for (int i = 0; i < 3; i++)
			{
				if (float3[i] < num)
				{
					num = float3[i];
					index = i;
				}
			}
			projectedPoint.Normal = float4.zero;
			projectedPoint.Point = point;
			projectedPoint.Normal[index] = ((point[index] > 0f) ? 1 : (-1));
			projectedPoint.Point[index] = float2[index] * projectedPoint.Normal[index];
		}
		else
		{
			projectedPoint.Point = math.clamp(point, -float2, float2);
			projectedPoint.Normal = math.normalizesafe(point - projectedPoint.Point);
		}
		projectedPoint.Point = ColliderToWorld.TransformPointUnscaled(projectedPoint.Point + @float + projectedPoint.Normal * Shape.ContactOffset);
		projectedPoint.Normal = ColliderToWorld.TransformDirection(projectedPoint.Normal);
	}
}
