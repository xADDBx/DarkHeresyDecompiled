using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Experimental.Collections;

[BurstCompile]
public struct UnsafeHeap<T, TLess> : IDisposable where T : unmanaged where TLess : unmanaged, IPredicate<T, T>
{
	private readonly Allocator m_Allocator;

	private TLess m_LessPredicate;

	private unsafe T* m_Buffer;

	private uint m_Capacity;

	private uint m_Length;

	public uint Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Capacity;
		}
	}

	public uint Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public UnsafeHeap(Allocator allocator)
		: this(0u, allocator)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe UnsafeHeap(uint initialCapacity, Allocator allocator)
	{
		m_Allocator = allocator;
		m_LessPredicate = new TLess();
		m_Length = 0u;
		if (initialCapacity != 0)
		{
			m_Capacity = math.ceilpow2(initialCapacity);
			m_Buffer = (T*)UnsafeUtility.MallocTracked(m_Capacity * sizeof(T), UnsafeUtility.AlignOf<T>(), m_Allocator, 0);
		}
		else
		{
			m_Capacity = 0u;
			m_Buffer = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Dispose()
	{
		if (m_Capacity != 0)
		{
			UnsafeUtility.FreeTracked(m_Buffer, m_Allocator);
			m_Capacity = 0u;
			m_Length = 0u;
			m_Buffer = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Push(T value)
	{
		if (m_Length == m_Capacity)
		{
			if (m_Capacity != 0)
			{
				uint num = m_Capacity * 2;
				T* ptr = (T*)UnsafeUtility.MallocTracked(num * sizeof(T), UnsafeUtility.AlignOf<T>(), m_Allocator, 0);
				UnsafeUtility.MemCpy(ptr, m_Buffer, m_Length * sizeof(T));
				UnsafeUtility.FreeTracked(m_Buffer, m_Allocator);
				m_Capacity = num;
				m_Buffer = ptr;
			}
			else
			{
				m_Capacity = (uint)(16 * sizeof(T));
				m_Buffer = (T*)UnsafeUtility.MallocTracked(m_Capacity * sizeof(T), UnsafeUtility.AlignOf<T>(), m_Allocator, 0);
			}
		}
		uint num2 = m_Length++;
		m_Buffer[num2] = value;
		while (num2 != 0)
		{
			uint parent = GetParent(num2);
			if (Less(in m_Buffer[parent], in value))
			{
				ref T reference = ref m_Buffer[parent];
				T* num3 = m_Buffer + num2;
				T val = m_Buffer[num2];
				T val2 = m_Buffer[parent];
				reference = val;
				*num3 = val2;
				num2 = parent;
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe T Top()
	{
		return *m_Buffer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Pop()
	{
		m_Length--;
		*m_Buffer = m_Buffer[m_Length];
		if (m_Length > 1)
		{
			MakeHeap();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint GetParent(uint i)
	{
		return (i - 1) / 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint GetLeftChild(uint i)
	{
		return 2 * i + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint GetRightChild(uint i)
	{
		return 2 * i + 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void MakeHeap()
	{
		uint num = 0u;
		uint leftChild = GetLeftChild(num);
		uint rightChild = GetRightChild(num);
		while (leftChild < m_Length || rightChild < m_Length)
		{
			uint num2 = num;
			if (leftChild < m_Length && Less(in m_Buffer[num2], in m_Buffer[leftChild]))
			{
				num2 = leftChild;
			}
			if (rightChild < m_Length && Less(in m_Buffer[num2], in m_Buffer[rightChild]))
			{
				num2 = rightChild;
			}
			if (num2 != num)
			{
				ref T reference = ref m_Buffer[num];
				T* num3 = m_Buffer + num2;
				T val = m_Buffer[num2];
				T val2 = m_Buffer[num];
				reference = val;
				*num3 = val2;
				num = num2;
				leftChild = GetLeftChild(num);
				rightChild = GetRightChild(num);
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Less(in T a, in T b)
	{
		return m_LessPredicate.Invoke(in a, in b);
	}

	[Conditional("ENABLE_ASSERT_INVARIANT")]
	private void AssertInvariant(uint i = 0u)
	{
	}
}
