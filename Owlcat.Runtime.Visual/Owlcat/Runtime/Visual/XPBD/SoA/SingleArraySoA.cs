using System.Runtime.InteropServices;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public class SingleArraySoA<T> : StructureOfArrays<T> where T : struct
{
	private NativeArray<T> m_Array;

	public NativeArray<T> Array => m_Array;

	public override T this[int index]
	{
		get
		{
			return m_Array[index];
		}
		set
		{
			m_Array[index] = value;
		}
	}

	public SingleArraySoA(int size)
		: base(size)
	{
		m_Allocator.Stride = Marshal.SizeOf(typeof(T));
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		m_Array = new NativeArray<T>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		SingleArraySoA<T> singleArraySoA = (SingleArraySoA<T>)dst;
		NativeArray<T>.Copy(m_Array, offset, singleArraySoA.m_Array, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		m_Array.Dispose();
	}

	public NativeSlice<T> GetSlice(int offset, int count)
	{
		return new NativeSlice<T>(m_Array, offset, count);
	}
}
