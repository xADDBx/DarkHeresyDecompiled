using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenIndexAllocator : IDisposable
{
	private struct Node
	{
		public int NextIndex;

		public int Generation;
	}

	public struct IndexAllocation : IEquatable<IndexAllocation>
	{
		public int Index;

		public int Generation;

		public static readonly IndexAllocation Invalid = new IndexAllocation
		{
			Index = -1,
			Generation = 0
		};

		public readonly bool IsValid()
		{
			return Index != -1;
		}

		public bool Equals(IndexAllocation other)
		{
			if (Index == other.Index)
			{
				return Generation == other.Generation;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is IndexAllocation other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Index, Generation);
		}
	}

	public const int kInvalidAllocationIndex = -1;

	private readonly bool m_AutoGrow;

	private int m_HeadIndex;

	private NativeArray<Node> m_Nodes;

	public int Capacity => m_Nodes.Length;

	public bool IsFull => m_HeadIndex == -1;

	public GPUDrivenIndexAllocator(int capacity, bool autoGrow = false)
	{
		m_AutoGrow = autoGrow;
		m_Nodes = new NativeArray<Node>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_HeadIndex = 0;
		InitNodesFrom(m_HeadIndex);
	}

	public void Dispose()
	{
		m_Nodes.Dispose();
	}

	private unsafe void InitNodesFrom(int startingNodeIndex)
	{
		Node* unsafePtr = (Node*)m_Nodes.GetUnsafePtr();
		for (int i = startingNodeIndex; i < m_Nodes.Length - 1; i++)
		{
			unsafePtr[i] = new Node
			{
				Generation = 0,
				NextIndex = i + 1
			};
		}
		unsafePtr[m_Nodes.Length - 1] = new Node
		{
			Generation = 0,
			NextIndex = -1
		};
	}

	public IndexAllocation Allocate()
	{
		if (IsFull)
		{
			if (m_AutoGrow)
			{
				ForceGrow(m_Nodes.Length * 2);
			}
			if (IsFull)
			{
				return IndexAllocation.Invalid;
			}
		}
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, m_HeadIndex);
		IndexAllocation indexAllocation = default(IndexAllocation);
		indexAllocation.Generation = reference.Generation;
		indexAllocation.Index = m_HeadIndex;
		IndexAllocation result = indexAllocation;
		m_HeadIndex = reference.NextIndex;
		reference.NextIndex = -1;
		return result;
	}

	public void ForceGrow(int ensuredCapacity)
	{
		if (ensuredCapacity > Capacity)
		{
			int capacity = Capacity;
			int capacity2 = Alignment.AlignUp(ensuredCapacity, capacity);
			ArrayExtensions.ResizeArray(ref m_Nodes, capacity2);
			m_HeadIndex = capacity;
			InitNodesFrom(m_HeadIndex);
		}
	}

	public int GetNodeGeneration(int index)
	{
		return m_Nodes[index].Generation;
	}

	public void Free(IndexAllocation allocation)
	{
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, allocation.Index);
		reference.NextIndex = m_HeadIndex;
		reference.Generation++;
		m_HeadIndex = allocation.Index;
	}

	public void FillMemoryCounter(Counters.CounterCollection counters, ProfilerCounterValue<int> counter)
	{
		counters.CollectBufferSize(counter, m_Nodes);
	}
}
