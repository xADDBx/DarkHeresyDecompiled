using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class MeshDeformerDescriptorSoA : StructureOfArrays<MeshDeformerDescriptor>
{
	public NativeArray<int2> DeformableVerticesRange;

	public NativeArray<int2> BindingsRange;

	public NativeArray<int2> VertexToSkinnedVertexMapRange;

	public NativeArray<float4x4> LocalToWorld;

	public NativeArray<int2> SkinnedVerticesRange;

	public NativeArray<float4x4> WorldToLocal;

	public override MeshDeformerDescriptor this[int index]
	{
		get
		{
			MeshDeformerDescriptor result = default(MeshDeformerDescriptor);
			result.DeformableVerticesRange = DeformableVerticesRange[index];
			result.BindingsRange = BindingsRange[index];
			result.VertexToSkinnedVertexMapRange = VertexToSkinnedVertexMapRange[index];
			result.LocalToWorld = LocalToWorld[index];
			result.SkinnedVerticesRange = SkinnedVerticesRange[index];
			result.WorldToLocal = WorldToLocal[index];
			return result;
		}
		set
		{
			DeformableVerticesRange[index] = value.DeformableVerticesRange;
			BindingsRange[index] = value.BindingsRange;
			VertexToSkinnedVertexMapRange[index] = value.VertexToSkinnedVertexMapRange;
			LocalToWorld[index] = value.LocalToWorld;
			SkinnedVerticesRange[index] = value.SkinnedVerticesRange;
			WorldToLocal[index] = value.WorldToLocal;
		}
	}

	public MeshDeformerDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		DeformableVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BindingsRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexToSkinnedVertexMapRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorld = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedVerticesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		WorldToLocal = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		MeshDeformerDescriptorSoA meshDeformerDescriptorSoA = (MeshDeformerDescriptorSoA)dst;
		NativeArray<int2>.Copy(DeformableVerticesRange, offset, meshDeformerDescriptorSoA.DeformableVerticesRange, dstOffset, length);
		NativeArray<int2>.Copy(BindingsRange, offset, meshDeformerDescriptorSoA.BindingsRange, dstOffset, length);
		NativeArray<int2>.Copy(VertexToSkinnedVertexMapRange, offset, meshDeformerDescriptorSoA.VertexToSkinnedVertexMapRange, dstOffset, length);
		NativeArray<float4x4>.Copy(LocalToWorld, offset, meshDeformerDescriptorSoA.LocalToWorld, dstOffset, length);
		NativeArray<int2>.Copy(SkinnedVerticesRange, offset, meshDeformerDescriptorSoA.SkinnedVerticesRange, dstOffset, length);
		NativeArray<float4x4>.Copy(WorldToLocal, offset, meshDeformerDescriptorSoA.WorldToLocal, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		DeformableVerticesRange.Dispose();
		BindingsRange.Dispose();
		VertexToSkinnedVertexMapRange.Dispose();
		LocalToWorld.Dispose();
		SkinnedVerticesRange.Dispose();
		WorldToLocal.Dispose();
	}

	public MeshDeformerDescriptorSoASlice GetSlice(int offset, int count)
	{
		MeshDeformerDescriptorSoASlice result = default(MeshDeformerDescriptorSoASlice);
		result.DeformableVerticesRange = new NativeSlice<int2>(DeformableVerticesRange, offset, count);
		result.BindingsRange = new NativeSlice<int2>(BindingsRange, offset, count);
		result.VertexToSkinnedVertexMapRange = new NativeSlice<int2>(VertexToSkinnedVertexMapRange, offset, count);
		result.LocalToWorld = new NativeSlice<float4x4>(LocalToWorld, offset, count);
		result.SkinnedVerticesRange = new NativeSlice<int2>(SkinnedVerticesRange, offset, count);
		result.WorldToLocal = new NativeSlice<float4x4>(WorldToLocal, offset, count);
		return result;
	}
}
