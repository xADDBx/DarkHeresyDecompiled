using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct BodyDescriptorSoASlice
{
	public NativeSlice<int2> SkinBufferRange;

	public NativeSlice<InertialForces> InertialForces;

	public NativeSlice<int> Enabled;

	public NativeSlice<float4x4> LocalToWorld;

	public NativeSlice<int2> VertexToParticleRange;

	public NativeSlice<int2> VertexTrianglesMapRangesRange;

	public NativeSlice<int2> ConstraintsRange;

	public NativeSlice<int2> TrianglesRange;

	public NativeSlice<int2> VertexTrianglesMapRange;

	public NativeSlice<float4> BodySimulationParameters;

	public NativeSlice<InertialFrame> InertialFrame;

	public NativeSlice<int2> MeshLocalVerticesRange;

	public NativeSlice<int2> BonesRange;

	public NativeSlice<int2> ParticlesRange;

	public NativeSlice<int2> BoneIndicesMapRange;

	public NativeSlice<int2> VerticesRange;

	public NativeSlice<float4x4> PrevWorldToLocal;

	public NativeSlice<int2> ConstraintBatchesRange;

	public NativeSlice<int2> SimplexConstraintsRange;

	public NativeSlice<int2> ParticleToVertexRange;

	public NativeSlice<int2> ConstraintSettingsRange;

	public NativeSlice<int2> BoneIndicesMapRangesRange;

	public NativeSlice<float4x4> WorldToLocal;

	public NativeSlice<int> Layer;

	public NativeSlice<uint> EnabledConstraintTypeMask;

	public NativeSlice<Aabb> Aabb;
}
