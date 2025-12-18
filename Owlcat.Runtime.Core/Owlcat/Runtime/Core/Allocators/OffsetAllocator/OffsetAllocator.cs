using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Allocators.OffsetAllocator;

[BurstCompile]
public struct OffsetAllocator : IDisposable
{
	private struct State
	{
		public uint FreeOffset;

		public uint FreeStorage;

		public uint UsedBinsTop;
	}

	public struct Allocation : IEquatable<Allocation>
	{
		public const uint kNoSpace = uint.MaxValue;

		public uint Offset;

		public uint Size;

		public uint Metadata;

		public static readonly Allocation Empty = new Allocation
		{
			Offset = uint.MaxValue,
			Size = uint.MaxValue,
			Metadata = uint.MaxValue
		};

		public bool Equals(Allocation other)
		{
			if (Offset == other.Offset && Size == other.Size)
			{
				return Metadata == other.Metadata;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Allocation other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)((((Offset * 397) ^ Size) * 397) ^ Metadata);
		}
	}

	public struct StorageReport
	{
		public uint TotalFreeSpace;

		public uint LargestFreeRegion;
	}

	public struct StorageReportFull : IDisposable
	{
		public struct Region
		{
			public uint Size;

			public uint Count;
		}

		public NativeArray<Region> FreeRegions;

		public StorageReportFull(Allocator allocator)
		{
			FreeRegions = new NativeArray<Region>(256, allocator);
		}

		public void Dispose()
		{
			FreeRegions.Dispose();
		}
	}

	private struct Node
	{
		public const uint kUnused = uint.MaxValue;

		public readonly uint DataOffset;

		public uint DataSize;

		public uint BinListPrev;

		public uint BinListNext;

		public uint NeighborPrev;

		public uint NeighborNext;

		public bool Used;

		public Node(uint dataOffset, uint dataSize)
		{
			DataOffset = dataOffset;
			DataSize = dataSize;
			BinListPrev = uint.MaxValue;
			BinListNext = uint.MaxValue;
			NeighborPrev = uint.MaxValue;
			NeighborNext = uint.MaxValue;
			Used = false;
		}
	}

	private const uint kNumTopBins = 32u;

	private const uint kBinsPerLeaf = 8u;

	private const int kTopBinsIndexShift = 3;

	private const uint kLeafBinsIndexMask = 7u;

	private const uint kNumLeafBins = 256u;

	public const uint kDefaultMaxAllocs = 131072u;

	private NativeArray<uint> m_BinIndices;

	private NativeArray<uint> m_FreeNodes;

	private NativeArray<Node> m_Nodes;

	private NativeArray<byte> m_UsedBins;

	private NativeReference<State> m_State;

	public int TotalUsedMemory => m_BinIndices.Length * UnsafeUtility.SizeOf<uint>() + m_FreeNodes.Length * UnsafeUtility.SizeOf<uint>() + m_Nodes.Length * UnsafeUtility.SizeOf<Node>() + m_UsedBins.Length * UnsafeUtility.SizeOf<byte>();

	public uint MaxAllocs { get; private set; }

	public uint Size { get; }

	public bool NoFreeAllocations => UnsafeCollectionExtensions.AsReadOnlyRef(in m_State).FreeOffset == 0;

	public OffsetAllocator(uint size, uint maxAllocs = 131072u)
	{
		Size = size;
		MaxAllocs = maxAllocs;
		m_Nodes = default(NativeArray<Node>);
		m_FreeNodes = default(NativeArray<uint>);
		m_UsedBins = new NativeArray<byte>(32, Allocator.Persistent);
		m_BinIndices = new NativeArray<uint>(256, Allocator.Persistent);
		m_State = new NativeReference<State>(default(State), Allocator.Persistent);
		Reset();
	}

	public void Dispose()
	{
		m_Nodes.Dispose();
		m_FreeNodes.Dispose();
		m_UsedBins.Dispose();
		m_BinIndices.Dispose();
		m_State.Dispose();
	}

	private static uint FindLowestSetBitAfter(uint bitMask, uint startBitIndex)
	{
		uint num = (uint)(~((1 << (int)startBitIndex) - 1));
		uint num2 = bitMask & num;
		if (num2 == 0)
		{
			return uint.MaxValue;
		}
		return Shared.tzcnt_nonzero(num2);
	}

	public unsafe void GrowMaxAllocs(uint newMaxAllocs)
	{
		if (MaxAllocs < newMaxAllocs)
		{
			newMaxAllocs = (uint)math.max(MaxAllocs, Alignment.AlignUp((int)newMaxAllocs, Alignment.AlignUp((int)MaxAllocs, 64)));
			ArrayExtensions.ResizeArray(ref m_Nodes, (int)newMaxAllocs);
			ArrayExtensions.ResizeArray(ref m_FreeNodes, (int)newMaxAllocs);
			uint num = newMaxAllocs - MaxAllocs;
			UnsafeUtility.MemCpy((byte*)m_FreeNodes.GetUnsafePtr() + (long)num * 4L, m_FreeNodes.GetUnsafePtr(), MaxAllocs * 4);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_FreeNodes, num2) = newMaxAllocs - num2 - 1;
			}
			UnsafeCollectionExtensions.AsRef(in m_State).FreeOffset += num;
			MaxAllocs = newMaxAllocs;
		}
	}

	public void Reset()
	{
		UnsafeCollectionExtensions.AsRef(in m_State) = new State
		{
			FreeStorage = 0u,
			UsedBinsTop = 0u,
			FreeOffset = MaxAllocs - 1
		};
		for (uint num = 0u; num < 32; num++)
		{
			m_UsedBins[(int)num] = 0;
		}
		for (uint num2 = 0u; num2 < 256; num2++)
		{
			m_BinIndices[(int)num2] = uint.MaxValue;
		}
		if (m_Nodes.IsCreated)
		{
			m_Nodes.Dispose();
		}
		if (m_FreeNodes.IsCreated)
		{
			m_FreeNodes.Dispose();
		}
		m_Nodes = new NativeArray<Node>((int)MaxAllocs, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_FreeNodes = new NativeArray<uint>((int)MaxAllocs, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (uint num3 = 0u; num3 < MaxAllocs; num3++)
		{
			m_FreeNodes[(int)num3] = MaxAllocs - num3 - 1;
		}
		InsertNodeIntoBin(Size, 0u);
	}

	public Allocation Allocate(uint size)
	{
		ref State reference = ref UnsafeCollectionExtensions.AsRef(in m_State);
		if (reference.FreeOffset == 0)
		{
			return Allocation.Empty;
		}
		uint num = SmallFloat.UintToFloatRoundUp(size);
		uint num2 = num >> 3;
		uint startBitIndex = num & 7u;
		uint num3 = num2;
		uint num4 = uint.MaxValue;
		if ((reference.UsedBinsTop & (1 << (int)num3)) != 0L)
		{
			num4 = FindLowestSetBitAfter(m_UsedBins[(int)num3], startBitIndex);
		}
		if (num4 == uint.MaxValue)
		{
			num3 = FindLowestSetBitAfter(reference.UsedBinsTop, num2 + 1);
			if (num3 == uint.MaxValue)
			{
				return Allocation.Empty;
			}
			num4 = Shared.tzcnt_nonzero(m_UsedBins[(int)num3]);
		}
		uint index = (num3 << 3) | num4;
		uint num5 = m_BinIndices[(int)index];
		ref Node reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num5);
		uint dataSize = reference2.DataSize;
		reference2.DataSize = size;
		reference2.Used = true;
		m_BinIndices[(int)index] = reference2.BinListNext;
		if (reference2.BinListNext != uint.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference2.BinListNext).BinListPrev = uint.MaxValue;
		}
		reference.FreeStorage -= dataSize;
		if (m_BinIndices[(int)index] == uint.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_UsedBins, num3) &= (byte)(~(1 << (int)num4));
			if (m_UsedBins[(int)num3] == 0)
			{
				reference.UsedBinsTop &= (uint)(~(1 << (int)num3));
			}
		}
		uint num6 = dataSize - size;
		if (num6 != 0)
		{
			uint num7 = InsertNodeIntoBin(num6, reference2.DataOffset + size);
			if (reference2.NeighborNext != uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference2.NeighborNext).NeighborPrev = num7;
			}
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num7).NeighborPrev = num5;
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num7).NeighborNext = reference2.NeighborNext;
			reference2.NeighborNext = num7;
		}
		Allocation result = default(Allocation);
		result.Offset = reference2.DataOffset;
		result.Size = size;
		result.Metadata = num5;
		return result;
	}

	public void Free(Allocation allocation)
	{
		if (m_Nodes.IsCreated)
		{
			uint metadata = allocation.Metadata;
			ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, metadata);
			uint dataOffset = reference.DataOffset;
			uint num = reference.DataSize;
			if (reference.NeighborPrev != uint.MaxValue && !UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.NeighborPrev).Used)
			{
				ref Node reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.NeighborPrev);
				dataOffset = reference2.DataOffset;
				num += reference2.DataSize;
				RemoveNodeFromBin(reference.NeighborPrev);
				reference.NeighborPrev = reference2.NeighborPrev;
			}
			if (reference.NeighborNext != uint.MaxValue && !UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.NeighborNext).Used)
			{
				ref Node reference3 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.NeighborNext);
				num += reference3.DataSize;
				RemoveNodeFromBin(reference.NeighborNext);
				reference.NeighborNext = reference3.NeighborNext;
			}
			uint neighborNext = reference.NeighborNext;
			uint neighborPrev = reference.NeighborPrev;
			UnsafeCollectionExtensions.ElementAsRef(in m_FreeNodes, ++UnsafeCollectionExtensions.AsRef(in m_State).FreeOffset) = metadata;
			uint num2 = InsertNodeIntoBin(num, dataOffset);
			if (neighborNext != uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num2).NeighborNext = neighborNext;
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, neighborNext).NeighborPrev = num2;
			}
			if (neighborPrev != uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num2).NeighborPrev = neighborPrev;
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, neighborPrev).NeighborNext = num2;
			}
		}
	}

	public uint AllocationSize(Allocation allocation)
	{
		if (allocation.Metadata == uint.MaxValue)
		{
			return 0u;
		}
		if (!m_Nodes.IsCreated)
		{
			return 0u;
		}
		return UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, allocation.Metadata).DataSize;
	}

	public StorageReport CreateStorageReport()
	{
		uint largestFreeRegion = 0u;
		uint totalFreeSpace = 0u;
		ref readonly State reference = ref UnsafeCollectionExtensions.AsReadOnlyRef(in m_State);
		if (reference.FreeOffset != 0)
		{
			totalFreeSpace = reference.FreeStorage;
			if (reference.UsedBinsTop != 0)
			{
				uint num = 31 - Shared.lzcnt_nonzero(reference.UsedBinsTop);
				uint num2 = 31 - Shared.lzcnt_nonzero(UnsafeCollectionExtensions.ElementAsRef(in m_UsedBins, num));
				largestFreeRegion = SmallFloat.FloatToUint((num << 3) | num2);
			}
		}
		StorageReport result = default(StorageReport);
		result.TotalFreeSpace = totalFreeSpace;
		result.LargestFreeRegion = largestFreeRegion;
		return result;
	}

	public StorageReportFull CreateStorageReportFull(Allocator allocator)
	{
		StorageReportFull result = new StorageReportFull(allocator);
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = 0u;
			uint num3 = UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num);
			while (num3 != uint.MaxValue)
			{
				num3 = UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num3).BinListNext;
				num2++;
			}
			UnsafeCollectionExtensions.ElementAsRef(in result.FreeRegions, num) = new StorageReportFull.Region
			{
				Size = SmallFloat.FloatToUint(num),
				Count = num2
			};
		}
		return result;
	}

	private uint InsertNodeIntoBin(uint size, uint dataOffset)
	{
		uint num = SmallFloat.UintToFloatRoundDown(size);
		uint num2 = num >> 3;
		uint num3 = num & 7u;
		ref State reference = ref UnsafeCollectionExtensions.AsRef(in m_State);
		if (UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num) == uint.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_UsedBins, num2) |= (byte)(1 << (int)num3);
			reference.UsedBinsTop |= (uint)(1 << (int)num2);
		}
		uint num4 = UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num);
		uint num5 = UnsafeCollectionExtensions.ElementAsRef(in m_FreeNodes, reference.FreeOffset--);
		UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num5) = new Node(dataOffset, size)
		{
			BinListNext = num4
		};
		if (num4 != uint.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num4).BinListPrev = num5;
		}
		UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num) = num5;
		reference.FreeStorage += size;
		return num5;
	}

	private void RemoveNodeFromBin(uint nodeIndex)
	{
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, nodeIndex);
		ref State reference2 = ref UnsafeCollectionExtensions.AsRef(in m_State);
		if (reference.BinListPrev != uint.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.BinListPrev).BinListNext = reference.BinListNext;
			if (reference.BinListNext != uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.BinListNext).BinListPrev = reference.BinListPrev;
			}
		}
		else
		{
			uint num = SmallFloat.UintToFloatRoundDown(reference.DataSize);
			uint num2 = num >> 3;
			uint num3 = num & 7u;
			UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num) = reference.BinListNext;
			if (reference.BinListNext != uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, reference.BinListNext).BinListPrev = uint.MaxValue;
			}
			if (UnsafeCollectionExtensions.ElementAsRef(in m_BinIndices, num) == uint.MaxValue)
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_UsedBins, num2) &= (byte)(~(1 << (int)num3));
				if (UnsafeCollectionExtensions.ElementAsRef(in m_UsedBins, num2) == 0)
				{
					reference2.UsedBinsTop &= (uint)(~(1 << (int)num2));
				}
			}
		}
		UnsafeCollectionExtensions.ElementAsRef(in m_FreeNodes, ++reference2.FreeOffset) = nodeIndex;
		reference2.FreeStorage -= reference.DataSize;
	}
}
