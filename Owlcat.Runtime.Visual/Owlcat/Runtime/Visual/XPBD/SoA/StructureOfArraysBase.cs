using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public abstract class StructureOfArraysBase
{
	protected MemoryAllocator m_Allocator;

	public int Capacity => m_Allocator.Size;

	public int Count => m_Allocator.Count();

	public int Stride => m_Allocator.Stride;

	public StructureOfArraysBase(int size)
	{
		m_Allocator = new MemoryAllocator(size);
		Resize(size);
	}

	public virtual void Dispose()
	{
	}

	public bool TryAlloc(int size, out int offset)
	{
		return m_Allocator.TryAlloc(size, out offset);
	}

	public void Free(int offset, int size)
	{
		m_Allocator.Free(offset, size);
	}

	public virtual void Resize(int newSize)
	{
		Dispose();
		m_Allocator.Resize(newSize);
	}

	public void Reset()
	{
		m_Allocator.Reset();
	}

	public int GetSizeInBytes()
	{
		if (m_Allocator.Stride > -1)
		{
			return m_Allocator.Size * m_Allocator.Stride;
		}
		return 0;
	}

	public abstract void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length);

	public virtual void Grow(int newSize)
	{
	}

	internal static void Grow<T>(ref NativeArray<T> array, int capacity, Allocator allocator) where T : struct
	{
		NativeArray<T> nativeArray = new NativeArray<T>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
		if (array.IsCreated)
		{
			NativeArray<T>.Copy(array, nativeArray, array.Length);
			array.Dispose();
		}
		array = nativeArray;
	}
}
