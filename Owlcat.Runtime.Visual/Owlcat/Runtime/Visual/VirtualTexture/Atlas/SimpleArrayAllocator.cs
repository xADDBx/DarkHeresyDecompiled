using System;
using Owlcat.Runtime.Core.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

public class SimpleArrayAllocator<T> : IDisposable where T : struct
{
	private NativeArray<T> m_Data;

	private NativeHashSet<int> m_Indices;

	private NativeList<int2> m_FreeBlocks;

	public NativeArray<T> Data => m_Data;

	public NativeHashSet<int> Indices => m_Indices;

	public NativeList<int2> FreeBlocks => m_FreeBlocks;

	public int DataLength => m_Data.Length;

	public int AllocatedCount => m_Indices.Count;

	public SimpleArrayAllocator(int capacity)
	{
		m_Data = new NativeArray<T>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Indices = new NativeHashSet<int>(capacity, Allocator.Persistent);
		m_FreeBlocks = new NativeList<int2>(4, Allocator.Persistent);
		ref NativeList<int2> freeBlocks = ref m_FreeBlocks;
		int2 value = new int2(0, capacity);
		freeBlocks.Add(in value);
	}

	public int Alloc(in T item)
	{
		if (m_FreeBlocks.Length <= 0)
		{
			int length = m_Data.Length;
			int num = (int)((float)length * 1.5f);
			ArrayExtensions.ResizeArray(ref m_Data, num);
			ref NativeList<int2> freeBlocks = ref m_FreeBlocks;
			int2 value = new int2(length, num - length);
			freeBlocks.Add(in value);
		}
		ref int2 reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, 0);
		int x = reference.x;
		m_Data[reference.x] = item;
		reference.x++;
		reference.y--;
		if (reference.y <= 0)
		{
			m_FreeBlocks.RemoveAt(0);
		}
		m_Indices.Add(x);
		return x;
	}

	public void Free(int index)
	{
		if (index < 0 || index >= m_Data.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (!m_Indices.Remove(index))
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (m_FreeBlocks.Length == 0)
		{
			ref NativeList<int2> freeBlocks = ref m_FreeBlocks;
			int2 value = new int2(index, 1);
			freeBlocks.Add(in value);
			return;
		}
		int num = -1;
		for (int i = 0; i < m_FreeBlocks.Length; i++)
		{
			ref int2 reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, i);
			if (index >= reference.x && index < reference.x + reference.y)
			{
				Debug.LogWarning("You are trying to free index that already free.");
				return;
			}
			if (index < reference.x)
			{
				m_FreeBlocks.InsertRange(i, 1);
				m_FreeBlocks[i] = new int2(index, 1);
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			ref int2 reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, num);
			if (num > 0)
			{
				ref int2 reference3 = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, num - 1);
				if (reference3.x + reference3.y >= reference2.x)
				{
					int num2 = reference2.x + reference2.y;
					reference2.x = reference3.x;
					reference2.y = num2 - reference2.x;
					m_FreeBlocks.RemoveAt(num - 1);
					num--;
					reference2 = ref m_FreeBlocks.ElementAt(num);
				}
			}
			if (num < m_FreeBlocks.Length - 1)
			{
				ref int2 reference4 = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, num + 1);
				if (reference2.x + reference2.y >= reference4.x)
				{
					reference2.y = reference4.x + reference4.y - reference2.x;
					m_FreeBlocks.RemoveAt(num + 1);
				}
			}
		}
		else
		{
			ref NativeList<int2> freeBlocks2 = ref m_FreeBlocks;
			int2 value = new int2(index, 1);
			freeBlocks2.Add(in value);
		}
	}

	public void Clear()
	{
		m_Indices.Clear();
		m_FreeBlocks.Clear();
		ref NativeList<int2> freeBlocks = ref m_FreeBlocks;
		int2 value = new int2(0, m_Data.Length);
		freeBlocks.Add(in value);
	}

	public bool IsFree(int index)
	{
		for (int i = 0; i < m_FreeBlocks.Length; i++)
		{
			ref int2 reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_FreeBlocks, i);
			if (index >= reference.x && index < reference.x + reference.y)
			{
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
		if (m_Data.IsCreated)
		{
			m_Data.Dispose();
		}
		if (m_Indices.IsCreated)
		{
			m_Indices.Dispose();
		}
		if (m_FreeBlocks.IsCreated)
		{
			m_FreeBlocks.Dispose();
		}
	}
}
