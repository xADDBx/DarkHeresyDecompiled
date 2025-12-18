using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Core.Collections;

[BurstCompile]
public static class UnsafeCollectionExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static UnsafeSpan<T> AsUnsafeSpan<T>(this in NativeArray<T> array) where T : unmanaged
	{
		return new UnsafeSpan<T>((T*)array.GetUnsafePtr(), array.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static UnsafeSpan<T> AsUnsafeSpan<T>(this in NativeList<T> list) where T : unmanaged
	{
		return new UnsafeSpan<T>(list.GetUnsafePtr(), list.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<T> AsSpan<T>(this in NativeList<T> list) where T : unmanaged
	{
		return new Span<T>(list.GetUnsafePtr(), list.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ReadOnlySpan<T> AsReadOnlySpan<T>(this in NativeList<T> list) where T : unmanaged
	{
		return new ReadOnlySpan<T>(list.GetUnsafeReadOnlyPtr(), list.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *reference.GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref readonly T AsReadOnlyRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *reference.GetUnsafeReadOnlyPtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRef<T>(this in NativeArray<T> nativeArray, int index) where T : struct
	{
		if (index < 0 || index >= nativeArray.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafePtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRefReadonly<T>(this in NativeArray<T> nativeArray, int index) where T : struct
	{
		if (index < 0 || index >= nativeArray.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafeReadOnlyPtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRefUnchecked<T>(this in NativeArray<T> nativeArray, int index) where T : struct
	{
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafePtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRef<T>(this in NativeSlice<T> nativeSlice, int index) where T : struct
	{
		if (index < 0 || index >= nativeSlice.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeSlice.GetUnsafePtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRefUnchecked<T>(this in NativeSlice<T> nativeSlice, int index) where T : struct
	{
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeSlice.GetUnsafePtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRef<T>(this in NativeArray<T> nativeArray, uint index) where T : struct
	{
		if (index >= nativeArray.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafePtr(), (int)index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref readonly T ElementAsRef<T>(this in NativeArray<T>.ReadOnly nativeArray, int index) where T : struct
	{
		if (index < 0 || index >= nativeArray.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafeReadOnlyPtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T ElementAsRef<T>(this in NativeList<T> nativeArray, int index) where T : unmanaged
	{
		if (index < 0 || index >= nativeArray.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafePtr(), index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void AddRange<T>(this NativeList<T> nativeList, UnsafeList<T> range) where T : unmanaged
	{
		nativeList.AddRange(range.Ptr, range.Length);
	}

	public unsafe static void CopyFrom<T>(this in NativeArray<T> destinationArray, NativeArray<T> sourceArray, int sourceOffset, int count) where T : unmanaged
	{
		if (sourceOffset + count > sourceArray.Length)
		{
			throw new ArgumentOutOfRangeException("sourceOffset");
		}
		T* source = (T*)((byte*)sourceArray.GetUnsafePtr() + (nint)sourceOffset * (nint)sizeof(T));
		UnsafeUtility.MemCpy(destinationArray.GetUnsafePtr(), source, count * UnsafeUtility.SizeOf<T>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ResizeArray<T>(this ref NativeArray<T> array, int capacity, Allocator allocator = Allocator.Persistent, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) where T : struct
	{
		NativeArray<T> nativeArray = new NativeArray<T>(capacity, allocator, options);
		if (array.IsCreated)
		{
			NativeArray<T>.Copy(array, nativeArray, array.Length);
			array.Dispose();
		}
		array = nativeArray;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ContainsValue<T, U>(this ref NativeParallelMultiHashMap<T, U> map, T key, U value) where T : unmanaged, IEquatable<T> where U : unmanaged, IEquatable<U>
	{
		foreach (U item in map.GetValuesForKey(key))
		{
			if (item.Equals(value))
			{
				return true;
			}
		}
		return false;
	}
}
