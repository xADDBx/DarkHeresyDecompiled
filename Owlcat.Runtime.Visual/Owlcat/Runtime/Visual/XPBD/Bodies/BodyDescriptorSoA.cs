using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class BodyDescriptorSoA : StructureOfArrays<BodyDescriptor>
{
	public NativeArray<int> Enabled;

	public NativeArray<float4> BodySimulationParameters;

	public NativeArray<float4x4> LocalToWorld;

	public NativeArray<Aabb> Aabb;

	public NativeArray<InertialForces> InertialForces;

	public NativeArray<float4x4> WorldToLocal;

	public NativeArray<int2> MeshLocalVerticesRange;

	public NativeArray<InertialFrame> InertialFrame;

	public NativeArray<int2> BoneIndicesMapRangesRange;

	public NativeArray<float4x4> PrevWorldToLocal;

	public NativeArray<int> Layer;

	public NativeArray<int2> VertexToParticleRange;

	public NativeArray<int2> TrianglesRange;

	public NativeArray<int2> ParticlesRange;

	public NativeArray<int2> BoneIndicesMapRange;

	public NativeArray<uint> EnabledConstraintTypeMask;

	public NativeArray<int2> BonesRange;

	public NativeArray<int2> VertexTrianglesMapRange;

	public NativeArray<int2> VerticesRange;

	public NativeArray<int2> ParticleToVertexRange;

	public NativeArray<int2> SkinBufferRange;

	public NativeArray<int2> SimplexConstraintsRange;

	public NativeArray<int2> VertexTrianglesMapRangesRange;

	public NativeArray<int2> ConstraintSettingsRange;

	public NativeArray<int2> ConstraintBatchesRange;

	public NativeArray<int2> ConstraintsRange;

	public override BodyDescriptor this[int index]
	{
		get
		{
			BodyDescriptor result = default(BodyDescriptor);
			result.Enabled = Enabled[index];
			result.BodySimulationParameters = BodySimulationParameters[index];
			result.LocalToWorld = LocalToWorld[index];
			result.Aabb = Aabb[index];
			result.InertialForces = InertialForces[index];
			result.WorldToLocal = WorldToLocal[index];
			result.MeshLocalVerticesRange = MeshLocalVerticesRange[index];
			result.InertialFrame = InertialFrame[index];
			result.BoneIndicesMapRangesRange = BoneIndicesMapRangesRange[index];
			result.PrevWorldToLocal = PrevWorldToLocal[index];
			result.Layer = Layer[index];
			result.VertexToParticleRange = VertexToParticleRange[index];
			result.TrianglesRange = TrianglesRange[index];
			result.ParticlesRange = ParticlesRange[index];
			result.BoneIndicesMapRange = BoneIndicesMapRange[index];
			result.EnabledConstraintTypeMask = EnabledConstraintTypeMask[index];
			result.BonesRange = BonesRange[index];
			result.VertexTrianglesMapRange = VertexTrianglesMapRange[index];
			result.VerticesRange = VerticesRange[index];
			result.ParticleToVertexRange = ParticleToVertexRange[index];
			result.SkinBufferRange = SkinBufferRange[index];
			result.SimplexConstraintsRange = SimplexConstraintsRange[index];
			result.VertexTrianglesMapRangesRange = VertexTrianglesMapRangesRange[index];
			result.ConstraintSettingsRange = ConstraintSettingsRange[index];
			result.ConstraintBatchesRange = ConstraintBatchesRange[index];
			result.ConstraintsRange = ConstraintsRange[index];
			return result;
		}
		set
		{
			Enabled[index] = value.Enabled;
			BodySimulationParameters[index] = value.BodySimulationParameters;
			LocalToWorld[index] = value.LocalToWorld;
			Aabb[index] = value.Aabb;
			InertialForces[index] = value.InertialForces;
			WorldToLocal[index] = value.WorldToLocal;
			MeshLocalVerticesRange[index] = value.MeshLocalVerticesRange;
			InertialFrame[index] = value.InertialFrame;
			BoneIndicesMapRangesRange[index] = value.BoneIndicesMapRangesRange;
			PrevWorldToLocal[index] = value.PrevWorldToLocal;
			Layer[index] = value.Layer;
			VertexToParticleRange[index] = value.VertexToParticleRange;
			TrianglesRange[index] = value.TrianglesRange;
			ParticlesRange[index] = value.ParticlesRange;
			BoneIndicesMapRange[index] = value.BoneIndicesMapRange;
			EnabledConstraintTypeMask[index] = value.EnabledConstraintTypeMask;
			BonesRange[index] = value.BonesRange;
			VertexTrianglesMapRange[index] = value.VertexTrianglesMapRange;
			VerticesRange[index] = value.VerticesRange;
			ParticleToVertexRange[index] = value.ParticleToVertexRange;
			SkinBufferRange[index] = value.SkinBufferRange;
			SimplexConstraintsRange[index] = value.SimplexConstraintsRange;
			VertexTrianglesMapRangesRange[index] = value.VertexTrianglesMapRangesRange;
			ConstraintSettingsRange[index] = value.ConstraintSettingsRange;
			ConstraintBatchesRange[index] = value.ConstraintBatchesRange;
			ConstraintsRange[index] = value.ConstraintsRange;
		}
	}

	public BodyDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float4>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<InertialForces>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<InertialFrame>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<uint>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Enabled = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BodySimulationParameters = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorld = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InertialForces = new NativeArray<InertialForces>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		WorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MeshLocalVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InertialFrame = new NativeArray<InertialFrame>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BoneIndicesMapRangesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevWorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexToParticleRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		TrianglesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticlesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BoneIndicesMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EnabledConstraintTypeMask = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BonesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTrianglesMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticleToVertexRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinBufferRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SimplexConstraintsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTrianglesMapRangesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintSettingsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintBatchesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		BodyDescriptorSoA bodyDescriptorSoA = (BodyDescriptorSoA)dst;
		NativeArray<int>.Copy(Enabled, offset, bodyDescriptorSoA.Enabled, dstOffset, length);
		NativeArray<float4>.Copy(BodySimulationParameters, offset, bodyDescriptorSoA.BodySimulationParameters, dstOffset, length);
		NativeArray<float4x4>.Copy(LocalToWorld, offset, bodyDescriptorSoA.LocalToWorld, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, bodyDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.InertialForces>.Copy(InertialForces, offset, bodyDescriptorSoA.InertialForces, dstOffset, length);
		NativeArray<float4x4>.Copy(WorldToLocal, offset, bodyDescriptorSoA.WorldToLocal, dstOffset, length);
		NativeArray<int2>.Copy(MeshLocalVerticesRange, offset, bodyDescriptorSoA.MeshLocalVerticesRange, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.InertialFrame>.Copy(InertialFrame, offset, bodyDescriptorSoA.InertialFrame, dstOffset, length);
		NativeArray<int2>.Copy(BoneIndicesMapRangesRange, offset, bodyDescriptorSoA.BoneIndicesMapRangesRange, dstOffset, length);
		NativeArray<float4x4>.Copy(PrevWorldToLocal, offset, bodyDescriptorSoA.PrevWorldToLocal, dstOffset, length);
		NativeArray<int>.Copy(Layer, offset, bodyDescriptorSoA.Layer, dstOffset, length);
		NativeArray<int2>.Copy(VertexToParticleRange, offset, bodyDescriptorSoA.VertexToParticleRange, dstOffset, length);
		NativeArray<int2>.Copy(TrianglesRange, offset, bodyDescriptorSoA.TrianglesRange, dstOffset, length);
		NativeArray<int2>.Copy(ParticlesRange, offset, bodyDescriptorSoA.ParticlesRange, dstOffset, length);
		NativeArray<int2>.Copy(BoneIndicesMapRange, offset, bodyDescriptorSoA.BoneIndicesMapRange, dstOffset, length);
		NativeArray<uint>.Copy(EnabledConstraintTypeMask, offset, bodyDescriptorSoA.EnabledConstraintTypeMask, dstOffset, length);
		NativeArray<int2>.Copy(BonesRange, offset, bodyDescriptorSoA.BonesRange, dstOffset, length);
		NativeArray<int2>.Copy(VertexTrianglesMapRange, offset, bodyDescriptorSoA.VertexTrianglesMapRange, dstOffset, length);
		NativeArray<int2>.Copy(VerticesRange, offset, bodyDescriptorSoA.VerticesRange, dstOffset, length);
		NativeArray<int2>.Copy(ParticleToVertexRange, offset, bodyDescriptorSoA.ParticleToVertexRange, dstOffset, length);
		NativeArray<int2>.Copy(SkinBufferRange, offset, bodyDescriptorSoA.SkinBufferRange, dstOffset, length);
		NativeArray<int2>.Copy(SimplexConstraintsRange, offset, bodyDescriptorSoA.SimplexConstraintsRange, dstOffset, length);
		NativeArray<int2>.Copy(VertexTrianglesMapRangesRange, offset, bodyDescriptorSoA.VertexTrianglesMapRangesRange, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintSettingsRange, offset, bodyDescriptorSoA.ConstraintSettingsRange, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintBatchesRange, offset, bodyDescriptorSoA.ConstraintBatchesRange, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintsRange, offset, bodyDescriptorSoA.ConstraintsRange, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Enabled.Dispose();
		BodySimulationParameters.Dispose();
		LocalToWorld.Dispose();
		Aabb.Dispose();
		InertialForces.Dispose();
		WorldToLocal.Dispose();
		MeshLocalVerticesRange.Dispose();
		InertialFrame.Dispose();
		BoneIndicesMapRangesRange.Dispose();
		PrevWorldToLocal.Dispose();
		Layer.Dispose();
		VertexToParticleRange.Dispose();
		TrianglesRange.Dispose();
		ParticlesRange.Dispose();
		BoneIndicesMapRange.Dispose();
		EnabledConstraintTypeMask.Dispose();
		BonesRange.Dispose();
		VertexTrianglesMapRange.Dispose();
		VerticesRange.Dispose();
		ParticleToVertexRange.Dispose();
		SkinBufferRange.Dispose();
		SimplexConstraintsRange.Dispose();
		VertexTrianglesMapRangesRange.Dispose();
		ConstraintSettingsRange.Dispose();
		ConstraintBatchesRange.Dispose();
		ConstraintsRange.Dispose();
	}

	public BodyDescriptorSoASlice GetSlice(int offset, int count)
	{
		BodyDescriptorSoASlice result = default(BodyDescriptorSoASlice);
		result.Enabled = new NativeSlice<int>(Enabled, offset, count);
		result.BodySimulationParameters = new NativeSlice<float4>(BodySimulationParameters, offset, count);
		result.LocalToWorld = new NativeSlice<float4x4>(LocalToWorld, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.InertialForces = new NativeSlice<InertialForces>(InertialForces, offset, count);
		result.WorldToLocal = new NativeSlice<float4x4>(WorldToLocal, offset, count);
		result.MeshLocalVerticesRange = new NativeSlice<int2>(MeshLocalVerticesRange, offset, count);
		result.InertialFrame = new NativeSlice<InertialFrame>(InertialFrame, offset, count);
		result.BoneIndicesMapRangesRange = new NativeSlice<int2>(BoneIndicesMapRangesRange, offset, count);
		result.PrevWorldToLocal = new NativeSlice<float4x4>(PrevWorldToLocal, offset, count);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.VertexToParticleRange = new NativeSlice<int2>(VertexToParticleRange, offset, count);
		result.TrianglesRange = new NativeSlice<int2>(TrianglesRange, offset, count);
		result.ParticlesRange = new NativeSlice<int2>(ParticlesRange, offset, count);
		result.BoneIndicesMapRange = new NativeSlice<int2>(BoneIndicesMapRange, offset, count);
		result.EnabledConstraintTypeMask = new NativeSlice<uint>(EnabledConstraintTypeMask, offset, count);
		result.BonesRange = new NativeSlice<int2>(BonesRange, offset, count);
		result.VertexTrianglesMapRange = new NativeSlice<int2>(VertexTrianglesMapRange, offset, count);
		result.VerticesRange = new NativeSlice<int2>(VerticesRange, offset, count);
		result.ParticleToVertexRange = new NativeSlice<int2>(ParticleToVertexRange, offset, count);
		result.SkinBufferRange = new NativeSlice<int2>(SkinBufferRange, offset, count);
		result.SimplexConstraintsRange = new NativeSlice<int2>(SimplexConstraintsRange, offset, count);
		result.VertexTrianglesMapRangesRange = new NativeSlice<int2>(VertexTrianglesMapRangesRange, offset, count);
		result.ConstraintSettingsRange = new NativeSlice<int2>(ConstraintSettingsRange, offset, count);
		result.ConstraintBatchesRange = new NativeSlice<int2>(ConstraintBatchesRange, offset, count);
		result.ConstraintsRange = new NativeSlice<int2>(ConstraintsRange, offset, count);
		return result;
	}
}
