using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[BurstCompile]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\XPBD\\Collisions\\Contact.cs", packingRules = PackingRules.Exact, needAccessors = false)]
public struct Contact : IComparable<Contact>
{
	public const int kMaxColliderContactsPerSilmpex = 4;

	public const int kMaxSimplexContactsPerSilmpex = 4;

	public const int kMaxContactsPerSimplex = 8;

	public float4 PointA;

	public float4 PointB;

	public float4 Normal;

	public int4 ParticleIndicesA;

	public int SimplexSizeA;

	public int4 ParticleIndicesB;

	public int SimplexSizeB;

	public float Distance;

	public float NormalLambda;

	public float TotalNormalInvMass;

	public int BodyA;

	public int BodyB;

	public float Friction;

	public static readonly Contact Empty = new Contact
	{
		BodyA = -1,
		BodyB = -1,
		SimplexSizeA = -1,
		SimplexSizeB = -1
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float SolvePenetration(in float4 posA, in float4 posB, in float maxDepenetrationDelta)
	{
		if (TotalNormalInvMass <= 0f)
		{
			return 0f;
		}
		Distance = math.dot(posA - posB, Normal);
		float num = math.max(0f - Distance - maxDepenetrationDelta, 0f);
		float num2 = (0f - (Distance + num)) / TotalNormalInvMass;
		float num3 = math.max(NormalLambda + num2, 0f);
		float result = num3 - NormalLambda;
		NormalLambda = num3;
		return result;
	}

	public int CompareTo(Contact other)
	{
		return ParticleIndicesA.x.CompareTo(other.ParticleIndicesA.x);
	}
}
