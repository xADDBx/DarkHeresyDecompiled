using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public class NativeMemoryAllocator
{
	private int m_Size;

	private SortedList<int, int> m_FreeBlocks = new SortedList<int, int>();

	private List<NativeReference<int2>> m_Allocations = new List<NativeReference<int2>>();

	public SortedList<int, int> FreeBlocks => m_FreeBlocks;

	public int Size => m_Size;

	public int Stride { get; set; } = -1;


	public NativeMemoryAllocator(int size)
	{
		Resize(size);
	}

	public void Dispose()
	{
		foreach (NativeReference<int2> allocation in m_Allocations)
		{
			allocation.Dispose();
		}
	}

	public void Reset()
	{
		m_FreeBlocks.Clear();
		m_FreeBlocks.Add(0, m_Size);
	}

	public void Resize(int newSize)
	{
		if (newSize > m_Size)
		{
			int value = newSize - m_Size;
			m_FreeBlocks.Add(m_Size, value);
			MergeFreeBlocks(m_Size);
			m_Size = newSize;
		}
		else if (newSize < m_Size)
		{
			m_Size = newSize;
			Reset();
		}
	}

	public NativeReference<int2> Alloc(int size)
	{
		if (!TryAlloc(size, out var offset))
		{
			Resize((int)((float)(m_Size + size) * 1.5f));
			TryAlloc(size, out offset);
		}
		NativeReference<int2> nativeReference = new NativeReference<int2>(new int2(offset, size), Allocator.Persistent);
		m_Allocations.Add(nativeReference);
		return nativeReference;
	}

	public bool TryAlloc(int size, out int offset)
	{
		offset = -1;
		if (size > m_Size)
		{
			return false;
		}
		foreach (KeyValuePair<int, int> freeBlock in m_FreeBlocks)
		{
			if (freeBlock.Value >= size)
			{
				offset = freeBlock.Key;
				break;
			}
		}
		if (offset > -1)
		{
			int num = m_FreeBlocks[offset];
			m_FreeBlocks.Remove(offset);
			if (num > size)
			{
				m_FreeBlocks.Add(offset + size, num - size);
			}
			return true;
		}
		return false;
	}

	public void Free(NativeReference<int2> allocation)
	{
		if (m_Allocations.Contains(allocation))
		{
			int2 value = allocation.Value;
			Free(value.x, value.y);
			m_Allocations.Remove(allocation);
			allocation.Dispose();
			return;
		}
		throw new ArgumentOutOfRangeException("allocation");
	}

	private void Free(int offset, int size)
	{
		if (offset >= m_Size)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset + size > m_Size)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (m_FreeBlocks.ContainsKey(offset))
		{
			if (m_FreeBlocks[offset] < size)
			{
				m_FreeBlocks[offset] = size;
			}
		}
		else
		{
			m_FreeBlocks.Add(offset, size);
		}
		MergeFreeBlocks(offset);
	}

	private void MergeFreeBlocks(int offset)
	{
		int num = m_FreeBlocks.IndexOfKey(offset);
		int num2 = num - 1;
		if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num2; i < m_FreeBlocks.Count - 1; i++)
		{
			int num3 = m_FreeBlocks.Keys[i];
			int num4 = m_FreeBlocks.Values[i];
			int num5 = m_FreeBlocks.Keys[i + 1];
			int num6 = m_FreeBlocks.Values[i + 1];
			if (num5 <= num3 + num4)
			{
				m_FreeBlocks.RemoveAt(i + 1);
				if (i <= num)
				{
					num--;
				}
				i--;
				if (num5 + num6 > num3 + num4)
				{
					m_FreeBlocks[num3] = num5 + num6 - num3;
				}
			}
			else if (i > num)
			{
				break;
			}
		}
	}

	public void Defragmentation()
	{
		while (m_FreeBlocks.Count > 1)
		{
			int num = m_FreeBlocks.Keys[0];
			int num2 = m_FreeBlocks.Values[0];
			for (int i = 0; i < m_Allocations.Count; i++)
			{
				NativeReference<int2> nativeReference = m_Allocations[i];
				int2 value = nativeReference.Value;
				if (value.x > num)
				{
					value.x -= num2;
				}
				nativeReference.Value = value;
			}
			m_FreeBlocks.Remove(num);
			m_FreeBlocks[m_FreeBlocks.Keys[m_FreeBlocks.Count - 1]] += num2;
		}
	}

	public int Count()
	{
		if (m_Size > 0)
		{
			int num = 0;
			for (int i = 0; i < m_FreeBlocks.Count; i++)
			{
				num += m_FreeBlocks.Values[i];
			}
			return m_Size - num;
		}
		return 0;
	}
}
