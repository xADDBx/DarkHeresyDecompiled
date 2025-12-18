using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.Experimental.Collections;

[BurstCompile]
public struct UnsafePriorityQueue<T> : IDisposable where T : unmanaged
{
	private struct Item
	{
		public T Value;

		public float Priority;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct ItemLess : IPredicate<Item, Item>
	{
		public bool Invoke(in Item a, in Item b)
		{
			return a.Priority < b.Priority;
		}

		bool IPredicate<Item, Item>.Invoke(in Item a, in Item b)
		{
			return Invoke(in a, in b);
		}
	}

	private UnsafeHeap<Item, ItemLess> m_Heap;

	public uint Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Heap.Capacity;
		}
	}

	public uint Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Heap.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public UnsafePriorityQueue(uint initialCapacity, Allocator allocator)
	{
		m_Heap = new UnsafeHeap<Item, ItemLess>(initialCapacity, allocator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		m_Heap.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Push(in T value, float priority)
	{
		m_Heap.Push(new Item
		{
			Value = value,
			Priority = priority
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Top()
	{
		return m_Heap.Top().Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Pop()
	{
		m_Heap.Pop();
	}
}
