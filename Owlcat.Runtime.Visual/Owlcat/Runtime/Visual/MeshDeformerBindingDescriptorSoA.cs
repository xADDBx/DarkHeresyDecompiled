using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual;

public class MeshDeformerBindingDescriptorSoA : StructureOfArrays<MeshDeformerBindingDescriptor>
{
	public NativeArray<float4x4> BodyToDeformer;

	public NativeArray<int> MasterIndexInSimulation;

	public NativeArray<int4> Offsets;

	public override MeshDeformerBindingDescriptor this[int index]
	{
		get
		{
			MeshDeformerBindingDescriptor result = default(MeshDeformerBindingDescriptor);
			result.BodyToDeformer = BodyToDeformer[index];
			result.MasterIndexInSimulation = MasterIndexInSimulation[index];
			result.Offsets = Offsets[index];
			return result;
		}
		set
		{
			BodyToDeformer[index] = value.BodyToDeformer;
			MasterIndexInSimulation[index] = value.MasterIndexInSimulation;
			Offsets[index] = value.Offsets;
		}
	}

	public MeshDeformerBindingDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<int4>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		BodyToDeformer = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MasterIndexInSimulation = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Offsets = new NativeArray<int4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		MeshDeformerBindingDescriptorSoA meshDeformerBindingDescriptorSoA = (MeshDeformerBindingDescriptorSoA)dst;
		NativeArray<float4x4>.Copy(BodyToDeformer, offset, meshDeformerBindingDescriptorSoA.BodyToDeformer, dstOffset, length);
		NativeArray<int>.Copy(MasterIndexInSimulation, offset, meshDeformerBindingDescriptorSoA.MasterIndexInSimulation, dstOffset, length);
		NativeArray<int4>.Copy(Offsets, offset, meshDeformerBindingDescriptorSoA.Offsets, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		BodyToDeformer.Dispose();
		MasterIndexInSimulation.Dispose();
		Offsets.Dispose();
	}

	public MeshDeformerBindingDescriptorSoASlice GetSlice(int offset, int count)
	{
		MeshDeformerBindingDescriptorSoASlice result = default(MeshDeformerBindingDescriptorSoASlice);
		result.BodyToDeformer = new NativeSlice<float4x4>(BodyToDeformer, offset, count);
		result.MasterIndexInSimulation = new NativeSlice<int>(MasterIndexInSimulation, offset, count);
		result.Offsets = new NativeSlice<int4>(Offsets, offset, count);
		return result;
	}
}
