using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[BurstCompile]
public struct BodyDescriptor
{
	public int2 ParticlesRange;

	public int2 ConstraintsRange;

	public int2 ConstraintBatchesRange;

	public int2 ConstraintSettingsRange;

	public int2 SimplexConstraintsRange;

	public int2 VertexToParticleRange;

	public int2 TrianglesRange;

	public int2 VertexTrianglesMapRange;

	public int2 VertexTrianglesMapRangesRange;

	public int2 VerticesRange;

	public int2 MeshLocalVerticesRange;

	public int2 ParticleToVertexRange;

	public int2 BonesRange;

	public int2 BoneIndicesMapRange;

	public int2 BoneIndicesMapRangesRange;

	public uint EnabledConstraintTypeMask;

	public int2 SkinBufferRange;

	public float4x4 WorldToLocal;

	public float4x4 LocalToWorld;

	public float4x4 PrevWorldToLocal;

	public Aabb Aabb;

	public float4 BodySimulationParameters;

	public InertialFrame InertialFrame;

	public InertialForces InertialForces;

	public int Layer;

	public int Enabled;

	public bool HasSimplices => SimplexConstraintsRange.x > -1;

	public bool HasVertices => VertexToParticleRange.x > -1;

	public bool HasSkinnedVertices => SkinBufferRange.x > -1;

	public bool HasBones => BonesRange.x > -1;

	public bool IsEnabled => Enabled > 0;
}
