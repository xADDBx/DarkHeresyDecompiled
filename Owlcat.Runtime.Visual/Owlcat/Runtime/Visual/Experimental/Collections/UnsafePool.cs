using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Experimental.Collections;

[BurstCompile]
public struct UnsafePool<T> : IDisposable where T : unmanaged
{
	private const uint kInvalidIndex = uint.MaxValue;

	public readonly Allocator Allocator;

	[NativeDisableUnsafePtrRestriction]
	public unsafe T* Buffer;

	public uint Length;

	public uint Next;

	public unsafe bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Buffer != null;
		}
	}

	public unsafe ref T this[uint index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref Buffer[index];
		}
	}

	public UnsafePool(Allocator allocator)
		: this(0u, allocator)
	{
	}

	public unsafe UnsafePool(uint initialCapacity, Allocator allocator)
	{
		Allocator = allocator;
		Buffer = null;
		Length = 0u;
		Next = uint.MaxValue;
		if (initialCapacity != 0)
		{
			Reallocate(math.ceilpow2(initialCapacity));
		}
	}

	public unsafe void Dispose()
	{
		if (Buffer != null)
		{
			UnsafeUtility.FreeTracked(Buffer, Allocator);
			Buffer = null;
		}
	}

	public void Clear()
	{
		if (Length != 0)
		{
			uint num = Length - 1;
			for (uint num2 = 0u; num2 < num; num2++)
			{
				GetNextPointerRef(num2) = num2 + 1;
			}
			GetNextPointerRef(num) = uint.MaxValue;
			Next = 0u;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint Allocate()
	{
		if (Next == uint.MaxValue)
		{
			uint newLength = ((Length == 0) ? 1u : (Length * 2));
			Reallocate(newLength);
		}
		uint next = Next;
		Next = GetNextPointerRef(Next);
		return next;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deallocate(uint index)
	{
		GetNextPointerRef(index) = Next;
		Next = index;
	}

	private unsafe void Reallocate(uint newLength)
	{
		Next = Length;
		T* ptr = (T*)UnsafeUtility.MallocTracked(newLength * sizeof(T), UnsafeUtility.AlignOf<T>(), Allocator, 0);
		if (Buffer != null)
		{
			UnsafeUtility.MemCpy(ptr, Buffer, Length * sizeof(T));
			UnsafeUtility.FreeTracked(Buffer, Allocator);
		}
		Buffer = ptr;
		Length = newLength;
		uint num = Length - 1;
		for (uint num2 = Next; num2 < num; num2++)
		{
			GetNextPointerRef(num2) = num2 + 1;
		}
		GetNextPointerRef(num) = uint.MaxValue;
	}

	private unsafe ref uint GetNextPointerRef(uint index)
	{
		return ref *(uint*)(Buffer + index);
	}
}
