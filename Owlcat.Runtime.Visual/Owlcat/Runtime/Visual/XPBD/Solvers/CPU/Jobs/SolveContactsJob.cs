using System.Runtime.CompilerServices;
using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct SolveContactsJob : IJobParallelForDefer
{
	public float StepTime;

	public float SubstepTime;

	public float MaxDepenetration;

	public float SubstepsToEnd;

	[NativeDisableParallelForRestriction]
	public NativeList<Contact> Contacts;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ParticlePrevPosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleInvMass;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> ParticleRadius;

	[NativeDisableParallelForRestriction]
	public NativeArray<int3> ParticleDeltas;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> ParticleCounts;

	public void Execute(int index)
	{
		Contact contact = Contacts[index];
		if (contact.SimplexSizeB < 0)
		{
			SolveColliderContact(ref contact);
		}
		else
		{
			SolveSimplexContact(ref contact);
		}
		Contacts[index] = contact;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveColliderContact(ref Contact contact)
	{
		float4 zero = float4.zero;
		float4 zero2 = float4.zero;
		float num = 0f;
		for (int i = 0; i < contact.SimplexSizeA; i++)
		{
			int index = contact.ParticleIndicesA[i];
			zero += new float4(ParticlePosition[index], 0f) * contact.PointA[i];
			zero2 += new float4(ParticlePrevPosition[index], 0f) * contact.PointA[i];
			num += ParticleRadius[index] * contact.PointA[i];
		}
		float4 posA = math.lerp(zero2, zero, SubstepsToEnd);
		posA += -contact.Normal * num;
		float4 posB = contact.PointB;
		float maxDepenetrationDelta = MaxDepenetration * StepTime;
		float num2 = contact.SolvePenetration(in posA, in posB, in maxDepenetrationDelta);
		if (math.abs(num2) > 1E-06f)
		{
			float4 @float = num2 * contact.Normal * XPBDMath.BaryScale(contact.PointA) / SubstepsToEnd;
			for (int j = 0; j < contact.SimplexSizeA; j++)
			{
				int index2 = contact.ParticleIndicesA[j];
				XPBDInterlocked.AddFloat3(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleDeltas, index2), @float.xyz * ParticleInvMass[index2] * contact.PointA[j]);
				Interlocked.Increment(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleCounts, index2));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SolveSimplexContact(ref Contact contact)
	{
		float4 zero = float4.zero;
		float num = 0f;
		for (int i = 0; i < contact.SimplexSizeA; i++)
		{
			int index = contact.ParticleIndicesA[i];
			zero += new float4(ParticlePosition[index], 0f) * contact.PointA[i];
			num += ParticleRadius[index] * contact.PointA[i];
		}
		float4 zero2 = float4.zero;
		float num2 = 0f;
		for (int j = 0; j < contact.SimplexSizeB; j++)
		{
			int index2 = contact.ParticleIndicesB[j];
			zero2 += new float4(ParticlePosition[index2], 0f) * contact.PointB[j];
			num2 += ParticleRadius[index2] * contact.PointB[j];
		}
		float4 posA = zero - contact.Normal * num;
		float4 posB = zero2 + contact.Normal * num2;
		float maxDepenetrationDelta = MaxDepenetration * SubstepTime;
		float num3 = contact.SolvePenetration(in posA, in posB, in maxDepenetrationDelta);
		if (math.abs(num3) > 1E-06f)
		{
			float4 @float = num3 * contact.Normal;
			float num4 = XPBDMath.BaryScale(contact.PointA);
			for (int k = 0; k < contact.SimplexSizeA; k++)
			{
				int index3 = contact.ParticleIndicesA[k];
				XPBDInterlocked.AddFloat3(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleDeltas, index3), @float.xyz * ParticleInvMass[index3] * contact.PointA[k] * num4);
				Interlocked.Increment(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleCounts, index3));
			}
			num4 = XPBDMath.BaryScale(contact.PointB);
			for (int l = 0; l < contact.SimplexSizeB; l++)
			{
				int index4 = contact.ParticleIndicesB[l];
				XPBDInterlocked.AddFloat3(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleDeltas, index4), -@float.xyz * ParticleInvMass[index4] * contact.PointB[l] * num4);
				Interlocked.Increment(ref UnsafeCollectionExtensions.ElementAsRef(in ParticleCounts, index4));
			}
		}
	}
}
