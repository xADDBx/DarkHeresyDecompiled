using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual;

[BurstCompile]
public struct UpdateMeshDeformerTransformsJob : IJobParallelForTransform
{
	[ReadOnly]
	public NativeArray<int> MeshDeformerIndicesMap;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> MeshDeformerLocalToWorld;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> MeshDeformerWorldToLocal;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<int2> MeshDeformerBindingsRange;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BindingsBodyToDeformer;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<int> BindingsMasterIndexInSimulation;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BodyLocalToWorld;

	public void Execute(int index, TransformAccess transform)
	{
		if (!transform.isValid)
		{
			return;
		}
		int index2 = MeshDeformerIndicesMap[index];
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		MeshDeformerLocalToWorld[index2] = localToWorldMatrix;
		MeshDeformerWorldToLocal[index2] = math.inverse(localToWorldMatrix);
		int2 @int = MeshDeformerBindingsRange[index2];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = @int.x + i;
			int num = BindingsMasterIndexInSimulation[index3];
			if (num > -1)
			{
				BindingsBodyToDeformer[index3] = math.mul(MeshDeformerWorldToLocal[index2], BodyLocalToWorld[num]);
			}
			else
			{
				BindingsBodyToDeformer[index3] = float4x4.identity;
			}
		}
	}
}
