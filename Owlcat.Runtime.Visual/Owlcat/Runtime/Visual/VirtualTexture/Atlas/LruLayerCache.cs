using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[BurstCompile]
public struct LruLayerCache : INativeDisposable, IDisposable
{
	public struct Node
	{
		public ushort Next;

		public ushort Prev;
	}

	public struct Layer
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe Node* Data;

		public ushort Length;

		public ushort GapOffset;

		public ushort GapStride;

		public ushort HeadId;

		public ushort TailId;

		public unsafe Layer([NotNull] Node* data, ushort length, ushort gapOffset, ushort gapStride)
		{
			BurstAssert.IsTrue(length > 0);
			Hint.Assume(length > 0);
			Data = data;
			Length = length;
			GapOffset = gapOffset;
			GapStride = gapStride;
			HeadId = 0;
			TailId = (ushort)(length - 1);
			for (int i = 0; i < length; i++)
			{
				Data[i].Prev = (ushort)PassGap((ushort)(i - 1), -1);
				Data[i].Next = (ushort)PassGap((ushort)(i + 1), 1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int PassGap(ushort index, int direction)
		{
			if (!IsGap(index))
			{
				return index;
			}
			if (direction > 0)
			{
				int num = (index / GapStride + direction) * GapStride;
				if (num >= Length - 1)
				{
					return -1;
				}
				return num;
			}
			return index / GapStride * GapStride + GapOffset - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsGap(ushort index)
		{
			if (index == ushort.MaxValue)
			{
				return false;
			}
			return index % GapStride >= GapOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void SetActive(ushort id)
		{
			BurstAssert.IsTrue(id < Length);
			Hint.Assume(id < Length);
			if (!IsGap(id) && id != TailId)
			{
				ref Node reference = ref Data[(int)id];
				if (HeadId == id)
				{
					HeadId = reference.Next;
				}
				else
				{
					Data[(int)reference.Prev].Next = reference.Next;
					Data[(int)reference.Next].Prev = reference.Prev;
				}
				reference.Prev = TailId;
				Data[(int)TailId].Next = id;
				TailId = id;
			}
		}

		public unsafe void Dump(List<int> results)
		{
			ushort num = HeadId;
			for (int i = 0; i < Length; i++)
			{
				results.Add(num);
				num = Data[(int)num].Next;
				if (num == TailId)
				{
					break;
				}
			}
		}
	}

	public const int kLayersCount = 4;

	private readonly ushort m_Length;

	private readonly ushort m_LineStride;

	private NativeArray<Node> m_NodesArray;

	private NativeArray<Layer> m_LayersArray;

	private Spinner m_Spinner;

	public ushort Length => m_Length;

	public LruLayerCache(ushort length, ushort lineStride)
	{
		BurstAssert.IsTrue(length >= 4);
		Hint.Assume(length >= 4);
		m_Length = length;
		m_LineStride = lineStride;
		m_NodesArray = new NativeArray<Node>(m_Length * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LayersArray = new NativeArray<Layer>(4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Spinner = default(Spinner);
		Init();
	}

	private unsafe void Init()
	{
		Node* unsafePtr = (Node*)m_NodesArray.GetUnsafePtr();
		for (int i = 0; i < 4; i++)
		{
			Node* data = unsafePtr + m_Length * i;
			ushort length = (ushort)(m_Length - i);
			ushort gapOffset = (ushort)(m_LineStride - i);
			m_LayersArray[i] = new Layer(data, length, gapOffset, m_LineStride);
		}
	}

	public void Dispose()
	{
		m_LayersArray.Dispose();
		m_NodesArray.Dispose();
	}

	public void Reset()
	{
		Dispose();
		m_NodesArray = new NativeArray<Node>(m_Length * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LayersArray = new NativeArray<Layer>(4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Init();
	}

	public JobHandle Dispose(JobHandle dependency)
	{
		return JobHandle.CombineDependencies(m_LayersArray.Dispose(dependency), m_NodesArray.Dispose(dependency));
	}

	public ushort GetMostOutdated(int layersCount)
	{
		BurstAssert.IsTrue(layersCount > 0);
		BurstAssert.IsTrue(layersCount <= 4);
		return m_LayersArray.AsReadOnlySpan()[layersCount - 1].HeadId;
	}

	public bool SetActive(int id, int layersCount)
	{
		BurstAssert.IsTrue(id >= 0);
		BurstAssert.IsTrue(id < m_Length);
		BurstAssert.IsTrue(layersCount > 0);
		BurstAssert.IsTrue(layersCount <= 4);
		if (id < 0)
		{
			return false;
		}
		if (id >= m_Length)
		{
			return false;
		}
		m_Spinner.Acquire();
		Span<Layer> span = m_LayersArray.AsSpan();
		int x = id + layersCount;
		for (int i = 0; i < span.Length; i++)
		{
			int num = math.max(id - i, 0);
			int num2 = math.min(x, span[i].Length);
			for (int j = num; j < num2; j++)
			{
				span[i].SetActive((ushort)j);
			}
		}
		m_Spinner.Release();
		return true;
	}

	public void Dump(int layersCount, List<int> results)
	{
		BurstAssert.IsTrue(layersCount > 0);
		BurstAssert.IsTrue(layersCount <= 4);
		m_LayersArray.AsSpan()[layersCount - 1].Dump(results);
	}
}
