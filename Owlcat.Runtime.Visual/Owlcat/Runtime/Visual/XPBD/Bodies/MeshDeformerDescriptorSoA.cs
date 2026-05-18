using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class MeshDeformerDescriptorSoA : StructureOfArrays<MeshDeformerDescriptor>
{
	public NativeArray<float4x4> WorldToLocal;

	public NativeArray<int2> BindingsRange;

	public NativeArray<float4x4> LocalToWorld;

	public NativeArray<int2> VertexToSkinnedVertexMapRange;

	public NativeArray<int2> DeformableVerticesRange;

	public NativeArray<int2> SkinnedVerticesRange;

	public override MeshDeformerDescriptor this[int index]
	{
		get
		{
			MeshDeformerDescriptor result = default(MeshDeformerDescriptor);
			result.WorldToLocal = WorldToLocal[index];
			result.BindingsRange = BindingsRange[index];
			result.LocalToWorld = LocalToWorld[index];
			result.VertexToSkinnedVertexMapRange = VertexToSkinnedVertexMapRange[index];
			result.DeformableVerticesRange = DeformableVerticesRange[index];
			result.SkinnedVerticesRange = SkinnedVerticesRange[index];
			return result;
		}
		set
		{
			WorldToLocal[index] = value.WorldToLocal;
			BindingsRange[index] = value.BindingsRange;
			LocalToWorld[index] = value.LocalToWorld;
			VertexToSkinnedVertexMapRange[index] = value.VertexToSkinnedVertexMapRange;
			DeformableVerticesRange[index] = value.DeformableVerticesRange;
			SkinnedVerticesRange[index] = value.SkinnedVerticesRange;
		}
	}

	public MeshDeformerDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		WorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BindingsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorld = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexToSkinnedVertexMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		DeformableVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		MeshDeformerDescriptorSoA meshDeformerDescriptorSoA = (MeshDeformerDescriptorSoA)dst;
		NativeArray<float4x4>.Copy(WorldToLocal, offset, meshDeformerDescriptorSoA.WorldToLocal, dstOffset, length);
		NativeArray<int2>.Copy(BindingsRange, offset, meshDeformerDescriptorSoA.BindingsRange, dstOffset, length);
		NativeArray<float4x4>.Copy(LocalToWorld, offset, meshDeformerDescriptorSoA.LocalToWorld, dstOffset, length);
		NativeArray<int2>.Copy(VertexToSkinnedVertexMapRange, offset, meshDeformerDescriptorSoA.VertexToSkinnedVertexMapRange, dstOffset, length);
		NativeArray<int2>.Copy(DeformableVerticesRange, offset, meshDeformerDescriptorSoA.DeformableVerticesRange, dstOffset, length);
		NativeArray<int2>.Copy(SkinnedVerticesRange, offset, meshDeformerDescriptorSoA.SkinnedVerticesRange, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		WorldToLocal.Dispose();
		BindingsRange.Dispose();
		LocalToWorld.Dispose();
		VertexToSkinnedVertexMapRange.Dispose();
		DeformableVerticesRange.Dispose();
		SkinnedVerticesRange.Dispose();
	}

	public MeshDeformerDescriptorSoASlice GetSlice(int offset, int count)
	{
		MeshDeformerDescriptorSoASlice result = default(MeshDeformerDescriptorSoASlice);
		result.WorldToLocal = new NativeSlice<float4x4>(WorldToLocal, offset, count);
		result.BindingsRange = new NativeSlice<int2>(BindingsRange, offset, count);
		result.LocalToWorld = new NativeSlice<float4x4>(LocalToWorld, offset, count);
		result.VertexToSkinnedVertexMapRange = new NativeSlice<int2>(VertexToSkinnedVertexMapRange, offset, count);
		result.DeformableVerticesRange = new NativeSlice<int2>(DeformableVerticesRange, offset, count);
		result.SkinnedVerticesRange = new NativeSlice<int2>(SkinnedVerticesRange, offset, count);
		return result;
	}
}
