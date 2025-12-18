using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

public class ConstraintSoA : StructureOfArrays<Constraint>
{
	public NativeArray<float4> Parameters1;

	public NativeArray<float4> Parameters0;

	public NativeArray<int4> Indices;

	public override Constraint this[int index]
	{
		get
		{
			Constraint result = default(Constraint);
			result.Parameters1 = Parameters1[index];
			result.Parameters0 = Parameters0[index];
			result.Indices = Indices[index];
			return result;
		}
		set
		{
			Parameters1[index] = value.Parameters1;
			Parameters0[index] = value.Parameters0;
			Indices[index] = value.Indices;
		}
	}

	public ConstraintSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float4>();
		num += Marshal.SizeOf<float4>();
		num += Marshal.SizeOf<int4>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Parameters1 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters0 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Indices = new NativeArray<int4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ConstraintSoA constraintSoA = (ConstraintSoA)dst;
		NativeArray<float4>.Copy(Parameters1, offset, constraintSoA.Parameters1, dstOffset, length);
		NativeArray<float4>.Copy(Parameters0, offset, constraintSoA.Parameters0, dstOffset, length);
		NativeArray<int4>.Copy(Indices, offset, constraintSoA.Indices, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Parameters1.Dispose();
		Parameters0.Dispose();
		Indices.Dispose();
	}

	public ConstraintSoASlice GetSlice(int offset, int count)
	{
		ConstraintSoASlice result = default(ConstraintSoASlice);
		result.Parameters1 = new NativeSlice<float4>(Parameters1, offset, count);
		result.Parameters0 = new NativeSlice<float4>(Parameters0, offset, count);
		result.Indices = new NativeSlice<int4>(Indices, offset, count);
		return result;
	}
}
