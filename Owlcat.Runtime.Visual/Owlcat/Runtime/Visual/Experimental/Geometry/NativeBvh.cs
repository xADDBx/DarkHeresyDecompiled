using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

[BurstCompile]
public struct NativeBvh<T> : IDisposable where T : unmanaged
{
	private readonly Allocator m_Allocator;

	[NativeDisableUnsafePtrRestriction]
	private unsafe UnsafeBvh<T>* m_Data;

	public unsafe bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data != null;
		}
	}

	public unsafe NativeBvh(uint initialCapacity, Allocator allocator)
	{
		m_Allocator = allocator;
		m_Data = (UnsafeBvh<T>*)UnsafeUtility.MallocTracked(sizeof(UnsafeBvh<T>), UnsafeUtility.AlignOf<UnsafeBvh<T>>(), allocator, 0);
		*m_Data = new UnsafeBvh<T>(initialCapacity, allocator);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			m_Data->Dispose();
			UnsafeUtility.FreeTracked(m_Data, m_Allocator);
			m_Data = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe uint Insert(T data, Aabb bounds)
	{
		return m_Data->Insert(data, bounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Delete(uint node)
	{
		m_Data->Delete(node);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Clear()
	{
		m_Data->Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe NativeBvhQueryContext<T> Query()
	{
		return new NativeBvhQueryContext<T>(*m_Data);
	}
}
