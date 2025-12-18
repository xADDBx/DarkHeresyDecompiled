using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct GenerateColliderContactsGlobalJob : IJobParallelFor
{
	public int2 ConstraintsRange;

	public int2 SimplexConstraintsRange;

	public int2 ConstraintSettingsRange;

	public int2 ParticlesRange;

	public int MaxContactsCount;

	public int OptimizationIterations;

	public float OptimizationTolerance;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeReference<int> ActiveContactsCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ConstraintSettings;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int4> SimplexIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> SimplexParameters0;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> SimplexParameters1;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Aabb> ColliderAabb;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<ColliderShape> ColliderShape;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform> ColliderTransform;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform> ColliderPrevTransform;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ColliderLayer;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleRadius;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	public AtomicCounter ContactsCounter;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeList<Contact>.ParallelWriter Contacts;

	[ReadOnly]
	public NativeReference<int> ObjectSizeSum;

	public int ObjectsCount;

	public int HashmapSize;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapKeys;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapValues;

	public void Execute(int index)
	{
		int simplexIndex = ConstraintsRange.x + SimplexConstraintsRange.x + index;
		float4 @float = ConstraintSettings[ConstraintSettingsRange.x + 4];
		int num = math.asint(@float.x);
		float friction = @float.y;
		float4 float2 = SimplexParameters0[simplexIndex];
		float4 float3 = SimplexParameters1[simplexIndex];
		float3 min = float2.xyz;
		float3 max = float3.xyz;
		Aabb bounds = new Aabb(in min, in max);
		float spacing = SpatialHashGrid.CalculateSpacing(ref ObjectSizeSum, in ObjectsCount);
		int3 @int = SpatialHashGrid.IntCoords(in bounds.Min, in spacing);
		int3 int2 = SpatialHashGrid.IntCoords(in bounds.Max, in spacing) + 1;
		int num2 = 0;
		int4 int3 = -1;
		for (int i = @int.z; i < int2.z; i++)
		{
			if (num2 >= 4)
			{
				break;
			}
			for (int j = @int.y; j < int2.y; j++)
			{
				if (num2 >= 4)
				{
					break;
				}
				for (int k = @int.x; k < int2.x; k++)
				{
					if (num2 >= 4)
					{
						break;
					}
					int num3 = SpatialHashGrid.HashCoords(ref k, ref j, ref i);
					int num4 = num3 % HashmapSize;
					int num5 = 0;
					while (num5 < 100 && num2 < 4)
					{
						int colliderIndex = HashmapValues[num4];
						if (colliderIndex == -1)
						{
							break;
						}
						if (HashmapKeys[num4] != num3)
						{
							num4 = (num4 + 1) % HashmapSize;
							num5++;
							continue;
						}
						if ((num & (1 << ColliderLayer[colliderIndex])) == 0)
						{
							num4 = (num4 + 1) % HashmapSize;
							num5++;
							continue;
						}
						if (!math.any(int3 == colliderIndex) && ColliderAabb[colliderIndex].IntersectsAabb(in bounds))
						{
							if (ContactsCounter.AddSat(1, MaxContactsCount) >= MaxContactsCount)
							{
								return;
							}
							Contacts.AddNoResize(GenerateContact(in simplexIndex, in colliderIndex, in friction));
							int3[num2] = colliderIndex;
							num2++;
						}
						num4 = (num4 + 1) % HashmapSize;
						num5++;
					}
				}
			}
		}
	}

	private Contact GenerateContact(in int simplexIndex, in int colliderIndex, in float friction)
	{
		int4 indices = SimplexIndices[simplexIndex];
		XPBDMath.GetSimplexIndicesAndSize(ref indices, in ParticlesRange.x, out var size);
		ColliderShape shape = ColliderShape[colliderIndex];
		Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform colliderToWorld = ColliderTransform[colliderIndex];
		Owlcat.Runtime.Visual.XPBD.DataStructures.AffineTransform colliderToWorldPrev = ColliderPrevTransform[colliderIndex];
		ShapeType shapeType = (ShapeType)shape.ShapeType;
		float4 convexBary = XPBDMath.BarycenterForSimplexOfSize(size);
		Contact contact = default(Contact);
		contact.BodyA = simplexIndex;
		contact.BodyB = colliderIndex;
		contact.ParticleIndicesA = indices;
		contact.SimplexSizeA = size;
		contact.SimplexSizeB = -1;
		Contact result = contact;
		switch (shapeType)
		{
		case ShapeType.Sphere:
		{
			SphereDF sphereDF = default(SphereDF);
			sphereDF.Shape = shape;
			sphereDF.ColliderToWorld = colliderToWorld;
			sphereDF.ColliderToWorldPrev = colliderToWorldPrev;
			SphereDF distanceFunction4 = sphereDF;
			float4 convexPoint4;
			LocalOptimization.SurfacePoint surfacePoint4 = LocalOptimization.Optimize(ref distanceFunction4, in indices, in size, ParticlePosition, ParticleRadius, ref convexBary, out convexPoint4, in OptimizationIterations, in OptimizationTolerance);
			result.PointB = surfacePoint4.Point;
			result.Normal = surfacePoint4.Normal;
			result.PointA = convexBary;
			break;
		}
		case ShapeType.Box:
		{
			BoxDF boxDF = default(BoxDF);
			boxDF.Shape = shape;
			boxDF.ColliderToWorld = colliderToWorld;
			boxDF.ColliderToWorldPrev = colliderToWorldPrev;
			BoxDF distanceFunction3 = boxDF;
			float4 convexPoint3;
			LocalOptimization.SurfacePoint surfacePoint3 = LocalOptimization.Optimize(ref distanceFunction3, in indices, in size, ParticlePosition, ParticleRadius, ref convexBary, out convexPoint3, in OptimizationIterations, in OptimizationTolerance);
			result.PointB = surfacePoint3.Point;
			result.Normal = surfacePoint3.Normal;
			result.PointA = convexBary;
			break;
		}
		case ShapeType.Capsule:
		{
			CapsuleDF capsuleDF = default(CapsuleDF);
			capsuleDF.Shape = shape;
			capsuleDF.ColliderToWorld = colliderToWorld;
			capsuleDF.ColliderToWorldPrev = colliderToWorldPrev;
			CapsuleDF distanceFunction2 = capsuleDF;
			float4 convexPoint2;
			LocalOptimization.SurfacePoint surfacePoint2 = LocalOptimization.Optimize(ref distanceFunction2, in indices, in size, ParticlePosition, ParticleRadius, ref convexBary, out convexPoint2, in OptimizationIterations, in OptimizationTolerance);
			result.PointB = surfacePoint2.Point;
			result.Normal = surfacePoint2.Normal;
			result.PointA = convexBary;
			break;
		}
		case ShapeType.FrustumCapsule:
		{
			FrustumCapsuleDF frustumCapsuleDF = default(FrustumCapsuleDF);
			frustumCapsuleDF.Shape = shape;
			frustumCapsuleDF.ColliderToWorld = colliderToWorld;
			frustumCapsuleDF.ColliderToWorldPrev = colliderToWorldPrev;
			FrustumCapsuleDF distanceFunction = frustumCapsuleDF;
			float4 convexPoint;
			LocalOptimization.SurfacePoint surfacePoint = LocalOptimization.Optimize(ref distanceFunction, in indices, in size, ParticlePosition, ParticleRadius, ref convexBary, out convexPoint, in OptimizationIterations, in OptimizationTolerance);
			result.PointB = surfacePoint.Point;
			result.Normal = surfacePoint.Normal;
			result.PointA = convexBary;
			break;
		}
		}
		float num = 0f;
		for (int i = 0; i < size; i++)
		{
			int index = indices[i];
			num += ParticleInvMass[index] * result.PointA[i];
		}
		result.TotalNormalInvMass = num;
		result.Friction = friction;
		Interlocked.Increment(ref UnsafeCollectionExtensions.AsRef(in ActiveContactsCount));
		return result;
	}
}
