using System;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Culling.Jobs;

[BurstCompile]
public struct CullBodiesJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<int> BodyVisibility;

	[WriteOnly]
	public NativeList<int>.ParallelWriter VisibleBodyIndices;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> BodyIndicesMap;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<Aabb> BodyAabbs;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> BodyEnabled;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeList<float4x4> CameraMatrices;

	public void Execute(int index)
	{
		int num = BodyIndicesMap[index];
		Aabb aabb = BodyAabbs[num];
		BodyVisibility[num] = 0;
		if (BodyEnabled[num] <= 0)
		{
			return;
		}
		for (int i = 0; i < CameraMatrices.Length; i++)
		{
			if (AabbInFrustum(CameraMatrices[i], aabb))
			{
				BodyVisibility[num] = 1;
				VisibleBodyIndices.AddNoResize(num);
				break;
			}
		}
	}

	private bool AabbInFrustum(float4x4 mvp, Aabb aabb)
	{
		Span<float4> span = stackalloc float4[8];
		span[0] = new float4(aabb.Min.x, aabb.Min.y, aabb.Min.z, 1f);
		span[1] = new float4(aabb.Min.x, aabb.Min.y, aabb.Max.z, 1f);
		span[2] = new float4(aabb.Min.x, aabb.Max.y, aabb.Min.z, 1f);
		span[3] = new float4(aabb.Min.x, aabb.Max.y, aabb.Max.z, 1f);
		span[4] = new float4(aabb.Max.x, aabb.Min.y, aabb.Min.z, 1f);
		span[5] = new float4(aabb.Max.x, aabb.Min.y, aabb.Max.z, 1f);
		span[6] = new float4(aabb.Max.x, aabb.Max.y, aabb.Min.z, 1f);
		span[7] = new float4(aabb.Max.x, aabb.Max.y, aabb.Max.z, 1f);
		span[0] = math.mul(mvp, span[0]);
		span[1] = math.mul(mvp, span[1]);
		span[2] = math.mul(mvp, span[2]);
		span[3] = math.mul(mvp, span[3]);
		span[4] = math.mul(mvp, span[4]);
		span[5] = math.mul(mvp, span[5]);
		span[6] = math.mul(mvp, span[6]);
		span[7] = math.mul(mvp, span[7]);
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			bool flag2 = span[0][i] > span[0].w && span[1][i] > span[1].w && span[2][i] > span[2].w && span[3][i] > span[3].w && span[4][i] > span[4].w && span[5][i] > span[5].w && span[6][i] > span[6].w && span[7][i] > span[7].w;
			bool flag3 = span[0][i] < 0f - span[0].w && span[1][i] < 0f - span[1].w && span[2][i] < 0f - span[2].w && span[3][i] < 0f - span[3].w && span[4][i] < 0f - span[4].w && span[5][i] < 0f - span[5].w && span[6][i] < 0f - span[6].w && span[7][i] < 0f - span[7].w;
			flag = flag || flag2 || flag3;
		}
		return !flag;
	}
}
