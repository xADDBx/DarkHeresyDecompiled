using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;

[BurstCompile]
public struct DeformMeshJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> MeshDeformerIndicesMap;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<int2> MeshDeformerBindingsRange;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<float4x4> BindingsBodyToDeformer;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<int4> BindingsOffsets;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> MeshBodyVertexPosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> MeshBodyVertexNormal;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<DeformableVertex> DeformableVertices;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<DeformableSkinnedVertex> SkinnedVertices;

	public void Execute(int index)
	{
		int index2 = MeshDeformerIndicesMap[index];
		int2 @int = MeshDeformerBindingsRange[index2];
		for (int i = 0; i < @int.y; i++)
		{
			int index3 = @int.x + i;
			float4x4 bodyToDeformer = BindingsBodyToDeformer[index3];
			int4 int2 = BindingsOffsets[index3];
			DeformMesh(in bodyToDeformer, int2.z, int2.x, int2.w, int2.y);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DeformMesh(in float4x4 bodyToDeformer, int verticesCount, int deformableVerticesOffset, int bodyVerticesOffset, int skinnedVerticesOffset)
	{
		for (int i = 0; i < verticesCount; i++)
		{
			int index = deformableVerticesOffset + i;
			DeformableVertex deformableVertex = DeformableVertices[index];
			int3 @int = deformableVertex.ParticleIndices + bodyVerticesOffset;
			float3 p = MeshBodyVertexPosition[@int.x];
			float3 p2 = MeshBodyVertexPosition[@int.y];
			float3 p3 = MeshBodyVertexPosition[@int.z];
			float3 n = MeshBodyVertexNormal[@int.x];
			float3 n2 = MeshBodyVertexNormal[@int.y];
			float3 n3 = MeshBodyVertexNormal[@int.z];
			DeformableSkinnedVertex value = default(DeformableSkinnedVertex);
			float3 coords = deformableVertex.PositionBary.xyz;
			XPBDMath.BarycentricInterpolation(in p, in p2, in p3, in n, in n2, in n3, in coords, in deformableVertex.PositionBary.w, out value.Position);
			coords = deformableVertex.NormalBary.xyz;
			XPBDMath.BarycentricInterpolation(in p, in p2, in p3, in n, in n2, in n3, in coords, in deformableVertex.NormalBary.w, out value.Normal);
			coords = deformableVertex.TangentBary.xyz;
			XPBDMath.BarycentricInterpolation(in p, in p2, in p3, in n, in n2, in n3, in coords, in deformableVertex.TangentBary.w, out value.Tangent);
			value.Normal -= value.Position;
			value.Tangent -= value.Position;
			value.Position = math.mul(bodyToDeformer, new float4(value.Position, 1f)).xyz;
			value.Normal = math.normalize(math.mul((float3x3)bodyToDeformer, value.Normal));
			value.Tangent = math.normalize(math.mul((float3x3)bodyToDeformer, value.Tangent));
			int index2 = skinnedVerticesOffset + i;
			SkinnedVertices[index2] = value;
		}
	}
}
