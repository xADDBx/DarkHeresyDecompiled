using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Owlcat.Runtime.Core.Collections;

[BurstCompile]
public struct FixedArray4<T> where T : unmanaged
{
	public T item0;

	public T item1;

	public T item2;

	public T item3;

	public unsafe ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			fixed (T* ptr = &item0)
			{
				return ref ptr[index];
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FixedArray4(in T splat)
	{
		item0 = (item1 = (item2 = (item3 = splat)));
	}

	public override bool Equals(object obj)
	{
		if (obj is FixedArray4<T> fixedArray && EqualityComparer<T>.Default.Equals(item0, fixedArray.item0) && EqualityComparer<T>.Default.Equals(item1, fixedArray.item1) && EqualityComparer<T>.Default.Equals(item2, fixedArray.item2))
		{
			return EqualityComparer<T>.Default.Equals(item3, fixedArray.item3);
		}
		return false;
	}

	public static bool operator ==(FixedArray4<T> a, FixedArray4<T> b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(FixedArray4<T> a, FixedArray4<T> b)
	{
		return !a.Equals(b);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(item0, item1, item2, item3);
	}
}
