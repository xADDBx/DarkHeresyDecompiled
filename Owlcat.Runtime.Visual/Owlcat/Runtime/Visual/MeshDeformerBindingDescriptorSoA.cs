using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual;

public class MeshDeformerBindingDescriptorSoA : StructureOfArrays<MeshDeformerBindingDescriptor>
{
	public NativeArray<int4> Offsets;

	public NativeArray<float4x4> BodyToDeformer;

	public NativeArray<int> MasterIndexInSimulation;

	public override MeshDeformerBindingDescriptor this[int index]
	{
		get
		{
			MeshDeformerBindingDescriptor result = default(MeshDeformerBindingDescriptor);
			result.Offsets = Offsets[index];
			result.BodyToDeformer = BodyToDeformer[index];
			result.MasterIndexInSimulation = MasterIndexInSimulation[index];
			return result;
		}
		set
		{
			Offsets[index] = value.Offsets;
			BodyToDeformer[index] = value.BodyToDeformer;
			MasterIndexInSimulation[index] = value.MasterIndexInSimulation;
		}
	}

	public MeshDeformerBindingDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int4>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Offsets = new NativeArray<int4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BodyToDeformer = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MasterIndexInSimulation = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		MeshDeformerBindingDescriptorSoA meshDeformerBindingDescriptorSoA = (MeshDeformerBindingDescriptorSoA)dst;
		NativeArray<int4>.Copy(Offsets, offset, meshDeformerBindingDescriptorSoA.Offsets, dstOffset, length);
		NativeArray<float4x4>.Copy(BodyToDeformer, offset, meshDeformerBindingDescriptorSoA.BodyToDeformer, dstOffset, length);
		NativeArray<int>.Copy(MasterIndexInSimulation, offset, meshDeformerBindingDescriptorSoA.MasterIndexInSimulation, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Offsets.Dispose();
		BodyToDeformer.Dispose();
		MasterIndexInSimulation.Dispose();
	}

	public MeshDeformerBindingDescriptorSoASlice GetSlice(int offset, int count)
	{
		MeshDeformerBindingDescriptorSoASlice result = default(MeshDeformerBindingDescriptorSoASlice);
		result.Offsets = new NativeSlice<int4>(Offsets, offset, count);
		result.BodyToDeformer = new NativeSlice<float4x4>(BodyToDeformer, offset, count);
		result.MasterIndexInSimulation = new NativeSlice<int>(MasterIndexInSimulation, offset, count);
		return result;
	}
}
