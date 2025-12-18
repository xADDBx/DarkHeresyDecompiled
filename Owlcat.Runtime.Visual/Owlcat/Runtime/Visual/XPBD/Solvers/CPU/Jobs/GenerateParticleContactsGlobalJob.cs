using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct GenerateParticleContactsGlobalJob : IJobParallelFor
{
	private float m_Friction;

	private ParticleCollisionMode m_CollisionMode;

	private int m_CollisionMask;

	public int2 ConstraintsRange;

	public int2 SimplexConstraintsRange;

	public int2 ConstraintSettingsRange;

	public int OptimizationIterations;

	public float OptimizationTolerance;

	public float Dt;

	public float CollisionMargin;

	public int MaxContactsCount;

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
	public NativeArray<float3> ParticlePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleBasePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleRadius;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticleVelocity;

	[ReadOnly]
	public NativeReference<int> ObjectSizeSum;

	public int HashmapSize;

	public int ObjectsCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapKeys;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapValues;

	public AtomicCounter ContactsCounter;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeList<Contact>.ParallelWriter Contacts;

	public void Execute(int index)
	{
		int num = ConstraintsRange.x + SimplexConstraintsRange.x + index;
		int4 indices = SimplexIndices[num];
		float4 @float = SimplexParameters0[num];
		float4 float2 = SimplexParameters1[num];
		int bodyLayer = (int)@float.w;
		int offset = (int)float2.w;
		XPBDMath.GetSimplexIndicesAndSize(ref indices, in offset, out var size);
		float4 float3 = ConstraintSettings[ConstraintSettingsRange.x + 4];
		m_CollisionMask = math.asint(float3.x);
		m_Friction = float3.y;
		m_CollisionMode = (ParticleCollisionMode)float3.z;
		float3 min = @float.xyz;
		float3 max = float2.xyz;
		Aabb aabb = new Aabb(in min, in max);
		float spacing = SpatialHashGrid.CalculateSpacing(ref ObjectSizeSum, in ObjectsCount);
		int3 @int = SpatialHashGrid.IntCoords(in aabb.Min, in spacing);
		int3 int2 = SpatialHashGrid.IntCoords(in aabb.Max, in spacing) + 1;
		int num2 = 0;
		Span<int> ints = stackalloc int[4];
		for (int i = 0; i < 4; i++)
		{
			ints[i] = -1;
		}
		Simplex simplex = default(Simplex);
		simplex.IndexInSolver = num;
		simplex.Size = size;
		simplex.ParticleIndices = indices;
		simplex.Aabb = aabb;
		simplex.BodyLayer = bodyLayer;
		Simplex simplexA = simplex;
		for (int j = @int.z; j < int2.z; j++)
		{
			if (num2 >= 4)
			{
				break;
			}
			for (int k = @int.y; k < int2.y; k++)
			{
				if (num2 >= 4)
				{
					break;
				}
				for (int l = @int.x; l < int2.x; l++)
				{
					if (num2 >= 4)
					{
						break;
					}
					int num3 = SpatialHashGrid.HashCoords(ref l, ref k, ref j);
					int num4 = num3 % HashmapSize;
					int num5 = 0;
					while (num5 < 100 && num2 < 4)
					{
						int num6 = HashmapValues[num4];
						if (num6 == -1)
						{
							break;
						}
						if (HashmapKeys[num4] != num3)
						{
							num4 = (num4 + 1) % HashmapSize;
							num5++;
							continue;
						}
						if (num == num6)
						{
							num4 = (num4 + 1) % HashmapSize;
							num5++;
							continue;
						}
						if (Contains(ints, num6))
						{
							num4 = (num4 + 1) % HashmapSize;
							num5++;
							continue;
						}
						int4 indices2 = SimplexIndices[num6];
						@float = SimplexParameters0[num6];
						float2 = SimplexParameters1[num6];
						bodyLayer = (int)@float.w;
						offset = (int)float2.w;
						XPBDMath.GetSimplexIndicesAndSize(ref indices2, in offset, out var size2);
						simplex = default(Simplex);
						simplex.IndexInSolver = num6;
						simplex.Size = size2;
						simplex.ParticleIndices = indices2;
						min = @float.xyz;
						max = float2.xyz;
						simplex.Aabb = new Aabb(in min, in max);
						simplex.BodyLayer = bodyLayer;
						Simplex simplexB = simplex;
						if (InteractionTest(in simplexA, in simplexB) && TryGenerateContact(in simplexA, in simplexB, out var contact))
						{
							if (ContactsCounter.AddSat(1, MaxContactsCount) >= MaxContactsCount)
							{
								return;
							}
							Contacts.AddNoResize(contact);
							ints[num2] = num6;
							num2++;
							Interlocked.Increment(ref UnsafeCollectionExtensions.AsRef(in ActiveContactsCount));
						}
						num4 = (num4 + 1) % HashmapSize;
						num5++;
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(Span<int> ints, int value)
	{
		for (int i = 0; i < ints.Length; i++)
		{
			if (ints[i] == value)
			{
				return true;
			}
			if (ints[i] < 0)
			{
				return false;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryGenerateContact(in Simplex simplexA, in Simplex simplexB, out Contact contact)
	{
		contact = default(Contact);
		float4 convexBary = XPBDMath.BarycenterForSimplexOfSize(simplexA.Size);
		SimplexDF distanceFunction = default(SimplexDF);
		distanceFunction.CacheData(in simplexB.ParticleIndices, in simplexB.Size, ParticlePosition, ParticleRadius);
		float4 convexPoint;
		LocalOptimization.SurfacePoint surfacePoint = LocalOptimization.Optimize(ref distanceFunction, in simplexA.ParticleIndices, in simplexA.Size, ParticlePosition, ParticleRadius, ref convexBary, out convexPoint, in OptimizationIterations, in OptimizationTolerance);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float4 zero = float4.zero;
		float4 zero2 = float4.zero;
		_ = float4.zero;
		for (int i = 0; i < simplexA.Size; i++)
		{
			int index = simplexA.ParticleIndices[i];
			num += ParticleRadius[index] * convexBary[i];
			zero.xyz += ParticleVelocity[index] * convexBary[i];
			num3 += ParticleInvMass[index] * convexBary[i];
		}
		for (int j = 0; j < simplexB.Size; j++)
		{
			int index2 = simplexB.ParticleIndices[j];
			num2 += ParticleRadius[index2] * surfacePoint.Bary[j];
			zero2.xyz += ParticleVelocity[index2] * surfacePoint.Bary[j];
			num3 += ParticleInvMass[index2] * surfacePoint.Bary[j];
		}
		float num4 = math.dot(convexPoint - surfacePoint.Point, surfacePoint.Normal);
		if (math.dot(zero - zero2, surfacePoint.Normal) * Dt + num4 <= num + num2 + CollisionMargin)
		{
			contact.BodyA = simplexA.IndexInSolver;
			contact.BodyB = simplexB.IndexInSolver;
			contact.SimplexSizeA = simplexA.Size;
			contact.SimplexSizeB = simplexB.Size;
			contact.ParticleIndicesA = simplexA.ParticleIndices;
			contact.ParticleIndicesB = simplexB.ParticleIndices;
			contact.Friction = m_Friction;
			contact.PointA = convexBary;
			contact.PointB = surfacePoint.Bary;
			contact.Normal = surfacePoint.Normal;
			contact.TotalNormalInvMass = num3;
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool InteractionTest(in Simplex simplexA, in Simplex simplexB)
	{
		int num = 0;
		for (int i = 0; i < simplexA.Size; i++)
		{
			for (int j = 0; j < simplexB.Size; j++)
			{
				if (simplexA.ParticleIndices[i] == simplexB.ParticleIndices[j])
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			return false;
		}
		if ((m_CollisionMask & (1 << simplexB.BodyLayer)) == 0)
		{
			return false;
		}
		if (!simplexA.Aabb.IntersectsAabb(in simplexB.Aabb))
		{
			return false;
		}
		int num2 = ConstraintsRange.x + SimplexConstraintsRange.x;
		int num3 = num2 + SimplexConstraintsRange.y;
		bool flag = simplexB.IndexInSolver >= num2 && simplexB.IndexInSolver < num3;
		switch (m_CollisionMode)
		{
		case ParticleCollisionMode.SelfOnly:
			if (!flag)
			{
				return false;
			}
			break;
		case ParticleCollisionMode.OtherOnly:
			if (flag)
			{
				return false;
			}
			break;
		default:
			return false;
		case ParticleCollisionMode.SelfAndOther:
			break;
		}
		return true;
	}
}
