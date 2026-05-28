using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class BodyDescriptorSoA : StructureOfArrays<BodyDescriptor>
{
	public NativeArray<float4x4> WorldToLocal;

	public NativeArray<int2> BoneIndicesMapRangesRange;

	public NativeArray<int2> MeshLocalVerticesRange;

	public NativeArray<int2> VertexToParticleRange;

	public NativeArray<float4x4> LocalToWorld;

	public NativeArray<int> Layer;

	public NativeArray<int2> BonesRange;

	public NativeArray<int2> SkinBufferRange;

	public NativeArray<int2> BoneIndicesMapRange;

	public NativeArray<int2> ConstraintSettingsRange;

	public NativeArray<int2> SimplexConstraintsRange;

	public NativeArray<int2> ConstraintBatchesRange;

	public NativeArray<int2> ParticleToVertexRange;

	public NativeArray<int2> ParticlesRange;

	public NativeArray<InertialForces> InertialForces;

	public NativeArray<Aabb> Aabb;

	public NativeArray<int2> VerticesRange;

	public NativeArray<InertialFrame> InertialFrame;

	public NativeArray<int2> TrianglesRange;

	public NativeArray<float4x4> PrevWorldToLocal;

	public NativeArray<int2> ConstraintsRange;

	public NativeArray<float4> BodySimulationParameters;

	public NativeArray<uint> EnabledConstraintTypeMask;

	public NativeArray<int2> VertexTrianglesMapRangesRange;

	public NativeArray<int> Enabled;

	public NativeArray<int2> VertexTrianglesMapRange;

	public override BodyDescriptor this[int index]
	{
		get
		{
			BodyDescriptor result = default(BodyDescriptor);
			result.WorldToLocal = WorldToLocal[index];
			result.BoneIndicesMapRangesRange = BoneIndicesMapRangesRange[index];
			result.MeshLocalVerticesRange = MeshLocalVerticesRange[index];
			result.VertexToParticleRange = VertexToParticleRange[index];
			result.LocalToWorld = LocalToWorld[index];
			result.Layer = Layer[index];
			result.BonesRange = BonesRange[index];
			result.SkinBufferRange = SkinBufferRange[index];
			result.BoneIndicesMapRange = BoneIndicesMapRange[index];
			result.ConstraintSettingsRange = ConstraintSettingsRange[index];
			result.SimplexConstraintsRange = SimplexConstraintsRange[index];
			result.ConstraintBatchesRange = ConstraintBatchesRange[index];
			result.ParticleToVertexRange = ParticleToVertexRange[index];
			result.ParticlesRange = ParticlesRange[index];
			result.InertialForces = InertialForces[index];
			result.Aabb = Aabb[index];
			result.VerticesRange = VerticesRange[index];
			result.InertialFrame = InertialFrame[index];
			result.TrianglesRange = TrianglesRange[index];
			result.PrevWorldToLocal = PrevWorldToLocal[index];
			result.ConstraintsRange = ConstraintsRange[index];
			result.BodySimulationParameters = BodySimulationParameters[index];
			result.EnabledConstraintTypeMask = EnabledConstraintTypeMask[index];
			result.VertexTrianglesMapRangesRange = VertexTrianglesMapRangesRange[index];
			result.Enabled = Enabled[index];
			result.VertexTrianglesMapRange = VertexTrianglesMapRange[index];
			return result;
		}
		set
		{
			WorldToLocal[index] = value.WorldToLocal;
			BoneIndicesMapRangesRange[index] = value.BoneIndicesMapRangesRange;
			MeshLocalVerticesRange[index] = value.MeshLocalVerticesRange;
			VertexToParticleRange[index] = value.VertexToParticleRange;
			LocalToWorld[index] = value.LocalToWorld;
			Layer[index] = value.Layer;
			BonesRange[index] = value.BonesRange;
			SkinBufferRange[index] = value.SkinBufferRange;
			BoneIndicesMapRange[index] = value.BoneIndicesMapRange;
			ConstraintSettingsRange[index] = value.ConstraintSettingsRange;
			SimplexConstraintsRange[index] = value.SimplexConstraintsRange;
			ConstraintBatchesRange[index] = value.ConstraintBatchesRange;
			ParticleToVertexRange[index] = value.ParticleToVertexRange;
			ParticlesRange[index] = value.ParticlesRange;
			InertialForces[index] = value.InertialForces;
			Aabb[index] = value.Aabb;
			VerticesRange[index] = value.VerticesRange;
			InertialFrame[index] = value.InertialFrame;
			TrianglesRange[index] = value.TrianglesRange;
			PrevWorldToLocal[index] = value.PrevWorldToLocal;
			ConstraintsRange[index] = value.ConstraintsRange;
			BodySimulationParameters[index] = value.BodySimulationParameters;
			EnabledConstraintTypeMask[index] = value.EnabledConstraintTypeMask;
			VertexTrianglesMapRangesRange[index] = value.VertexTrianglesMapRangesRange;
			Enabled[index] = value.Enabled;
			VertexTrianglesMapRange[index] = value.VertexTrianglesMapRange;
		}
	}

	public BodyDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<InertialForces>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<InertialFrame>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4>();
		num += Marshal.SizeOf<uint>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<int2>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		WorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BoneIndicesMapRangesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MeshLocalVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexToParticleRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorld = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BonesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinBufferRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BoneIndicesMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintSettingsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SimplexConstraintsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintBatchesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticleToVertexRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticlesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InertialForces = new NativeArray<InertialForces>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InertialFrame = new NativeArray<InertialFrame>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		TrianglesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevWorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BodySimulationParameters = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EnabledConstraintTypeMask = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTrianglesMapRangesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Enabled = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTrianglesMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		BodyDescriptorSoA bodyDescriptorSoA = (BodyDescriptorSoA)dst;
		NativeArray<float4x4>.Copy(WorldToLocal, offset, bodyDescriptorSoA.WorldToLocal, dstOffset, length);
		NativeArray<int2>.Copy(BoneIndicesMapRangesRange, offset, bodyDescriptorSoA.BoneIndicesMapRangesRange, dstOffset, length);
		NativeArray<int2>.Copy(MeshLocalVerticesRange, offset, bodyDescriptorSoA.MeshLocalVerticesRange, dstOffset, length);
		NativeArray<int2>.Copy(VertexToParticleRange, offset, bodyDescriptorSoA.VertexToParticleRange, dstOffset, length);
		NativeArray<float4x4>.Copy(LocalToWorld, offset, bodyDescriptorSoA.LocalToWorld, dstOffset, length);
		NativeArray<int>.Copy(Layer, offset, bodyDescriptorSoA.Layer, dstOffset, length);
		NativeArray<int2>.Copy(BonesRange, offset, bodyDescriptorSoA.BonesRange, dstOffset, length);
		NativeArray<int2>.Copy(SkinBufferRange, offset, bodyDescriptorSoA.SkinBufferRange, dstOffset, length);
		NativeArray<int2>.Copy(BoneIndicesMapRange, offset, bodyDescriptorSoA.BoneIndicesMapRange, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintSettingsRange, offset, bodyDescriptorSoA.ConstraintSettingsRange, dstOffset, length);
		NativeArray<int2>.Copy(SimplexConstraintsRange, offset, bodyDescriptorSoA.SimplexConstraintsRange, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintBatchesRange, offset, bodyDescriptorSoA.ConstraintBatchesRange, dstOffset, length);
		NativeArray<int2>.Copy(ParticleToVertexRange, offset, bodyDescriptorSoA.ParticleToVertexRange, dstOffset, length);
		NativeArray<int2>.Copy(ParticlesRange, offset, bodyDescriptorSoA.ParticlesRange, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.InertialForces>.Copy(InertialForces, offset, bodyDescriptorSoA.InertialForces, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, bodyDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<int2>.Copy(VerticesRange, offset, bodyDescriptorSoA.VerticesRange, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.InertialFrame>.Copy(InertialFrame, offset, bodyDescriptorSoA.InertialFrame, dstOffset, length);
		NativeArray<int2>.Copy(TrianglesRange, offset, bodyDescriptorSoA.TrianglesRange, dstOffset, length);
		NativeArray<float4x4>.Copy(PrevWorldToLocal, offset, bodyDescriptorSoA.PrevWorldToLocal, dstOffset, length);
		NativeArray<int2>.Copy(ConstraintsRange, offset, bodyDescriptorSoA.ConstraintsRange, dstOffset, length);
		NativeArray<float4>.Copy(BodySimulationParameters, offset, bodyDescriptorSoA.BodySimulationParameters, dstOffset, length);
		NativeArray<uint>.Copy(EnabledConstraintTypeMask, offset, bodyDescriptorSoA.EnabledConstraintTypeMask, dstOffset, length);
		NativeArray<int2>.Copy(VertexTrianglesMapRangesRange, offset, bodyDescriptorSoA.VertexTrianglesMapRangesRange, dstOffset, length);
		NativeArray<int>.Copy(Enabled, offset, bodyDescriptorSoA.Enabled, dstOffset, length);
		NativeArray<int2>.Copy(VertexTrianglesMapRange, offset, bodyDescriptorSoA.VertexTrianglesMapRange, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		WorldToLocal.Dispose();
		BoneIndicesMapRangesRange.Dispose();
		MeshLocalVerticesRange.Dispose();
		VertexToParticleRange.Dispose();
		LocalToWorld.Dispose();
		Layer.Dispose();
		BonesRange.Dispose();
		SkinBufferRange.Dispose();
		BoneIndicesMapRange.Dispose();
		ConstraintSettingsRange.Dispose();
		SimplexConstraintsRange.Dispose();
		ConstraintBatchesRange.Dispose();
		ParticleToVertexRange.Dispose();
		ParticlesRange.Dispose();
		InertialForces.Dispose();
		Aabb.Dispose();
		VerticesRange.Dispose();
		InertialFrame.Dispose();
		TrianglesRange.Dispose();
		PrevWorldToLocal.Dispose();
		ConstraintsRange.Dispose();
		BodySimulationParameters.Dispose();
		EnabledConstraintTypeMask.Dispose();
		VertexTrianglesMapRangesRange.Dispose();
		Enabled.Dispose();
		VertexTrianglesMapRange.Dispose();
	}

	public BodyDescriptorSoASlice GetSlice(int offset, int count)
	{
		BodyDescriptorSoASlice result = default(BodyDescriptorSoASlice);
		result.WorldToLocal = new NativeSlice<float4x4>(WorldToLocal, offset, count);
		result.BoneIndicesMapRangesRange = new NativeSlice<int2>(BoneIndicesMapRangesRange, offset, count);
		result.MeshLocalVerticesRange = new NativeSlice<int2>(MeshLocalVerticesRange, offset, count);
		result.VertexToParticleRange = new NativeSlice<int2>(VertexToParticleRange, offset, count);
		result.LocalToWorld = new NativeSlice<float4x4>(LocalToWorld, offset, count);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.BonesRange = new NativeSlice<int2>(BonesRange, offset, count);
		result.SkinBufferRange = new NativeSlice<int2>(SkinBufferRange, offset, count);
		result.BoneIndicesMapRange = new NativeSlice<int2>(BoneIndicesMapRange, offset, count);
		result.ConstraintSettingsRange = new NativeSlice<int2>(ConstraintSettingsRange, offset, count);
		result.SimplexConstraintsRange = new NativeSlice<int2>(SimplexConstraintsRange, offset, count);
		result.ConstraintBatchesRange = new NativeSlice<int2>(ConstraintBatchesRange, offset, count);
		result.ParticleToVertexRange = new NativeSlice<int2>(ParticleToVertexRange, offset, count);
		result.ParticlesRange = new NativeSlice<int2>(ParticlesRange, offset, count);
		result.InertialForces = new NativeSlice<InertialForces>(InertialForces, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.VerticesRange = new NativeSlice<int2>(VerticesRange, offset, count);
		result.InertialFrame = new NativeSlice<InertialFrame>(InertialFrame, offset, count);
		result.TrianglesRange = new NativeSlice<int2>(TrianglesRange, offset, count);
		result.PrevWorldToLocal = new NativeSlice<float4x4>(PrevWorldToLocal, offset, count);
		result.ConstraintsRange = new NativeSlice<int2>(ConstraintsRange, offset, count);
		result.BodySimulationParameters = new NativeSlice<float4>(BodySimulationParameters, offset, count);
		result.EnabledConstraintTypeMask = new NativeSlice<uint>(EnabledConstraintTypeMask, offset, count);
		result.VertexTrianglesMapRangesRange = new NativeSlice<int2>(VertexTrianglesMapRangesRange, offset, count);
		result.Enabled = new NativeSlice<int>(Enabled, offset, count);
		result.VertexTrianglesMapRange = new NativeSlice<int2>(VertexTrianglesMapRange, offset, count);
		return result;
	}
}
