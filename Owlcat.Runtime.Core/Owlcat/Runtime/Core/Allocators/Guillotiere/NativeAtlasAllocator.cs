using System;
using Owlcat.Runtime.Core.Collections;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

public struct NativeAtlasAllocator : IDisposable
{
	private const int kNone = int.MaxValue;

	private NativeList<Node> m_Nodes;

	private NativeList<int> m_Generations;

	private int m_UnusedNodes;

	private int m_RootNode;

	private NativeList<int> m_SmallBucket;

	private NativeList<int> m_MediumBucket;

	private NativeList<int> m_LargeBucket;

	private int2 m_Alignment;

	private int m_SmallSizeThreshold;

	private int m_LargeSizeThreshold;

	private int2 m_Size;

	private const int LARGE_BUCKET = 2;

	private const int MEDIUM_BUCKET = 1;

	private const int SMALL_BUCKET = 0;

	private const int NUM_BUCKETS = 3;

	private const uint GEN_MASK = 4278190080u;

	private const uint IDX_MASK = 16777215u;

	public NativeList<Node> Nodes => m_Nodes;

	public int Width => m_Size.x;

	public int Height => m_Size.y;

	public NativeAtlasAllocator(int2 size, in AllocatorOptions options)
	{
		m_Size = size;
		m_Alignment = options.Alignment;
		m_LargeSizeThreshold = options.LargeSizeThreshold;
		m_SmallSizeThreshold = options.SmallSizeThreshold;
		m_Nodes = new NativeList<Node>(16, Allocator.Persistent);
		NativeList<int> generations = new NativeList<int>(16, Allocator.Persistent);
		int value = 0;
		generations.Add(in value);
		m_Generations = generations;
		ref NativeList<Node> nodes = ref m_Nodes;
		Node value2 = new Node
		{
			Parent = int.MaxValue,
			NextSibling = int.MaxValue,
			PrevSibling = int.MaxValue,
			Rect = new NativeRectInt(int2.zero, m_Size),
			Kind = NodeKind.Free,
			Orientation = Orientation.Vertical
		};
		nodes.Add(in value2);
		generations = new NativeList<int>(16, Allocator.Persistent);
		value = 0;
		generations.Add(in value);
		m_SmallBucket = generations;
		generations = new NativeList<int>(16, Allocator.Persistent);
		value = 0;
		generations.Add(in value);
		m_MediumBucket = generations;
		generations = new NativeList<int>(16, Allocator.Persistent);
		value = 0;
		generations.Add(in value);
		m_LargeBucket = generations;
		m_UnusedNodes = int.MaxValue;
		m_RootNode = 0;
	}

	public NativeAtlasAllocator(int2 size)
		: this(size, in AllocatorOptions.DefaultOptions)
	{
	}

	public void Dispose()
	{
		if (m_Nodes.IsCreated)
		{
			m_Nodes.Dispose();
		}
		if (m_Generations.IsCreated)
		{
			m_Generations.Dispose();
		}
		if (m_SmallBucket.IsCreated)
		{
			m_SmallBucket.Dispose();
		}
		if (m_MediumBucket.IsCreated)
		{
			m_MediumBucket.Dispose();
		}
		if (m_LargeBucket.IsCreated)
		{
			m_LargeBucket.Dispose();
		}
	}

	public Allocation Allocate(int2 requestedSize)
	{
		if (requestedSize.x <= 0 || requestedSize.y <= 0)
		{
			return Allocation.Empty;
		}
		AdjustSize(m_Alignment.x, ref requestedSize.x);
		AdjustSize(m_Alignment.y, ref requestedSize.y);
		int num = FindSuitableRect(requestedSize);
		if (num == int.MaxValue)
		{
			return Allocation.Empty;
		}
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num);
		NativeRectInt chosenRect = reference.Rect;
		NativeRectInt rect = new NativeRectInt(chosenRect.position, requestedSize);
		Orientation orientation = reference.Orientation;
		GuillotineRect(in chosenRect, requestedSize, orientation, out var splitRect, out var leftoverRect, out var orientation2);
		int num2;
		int num3;
		int num4;
		if (orientation2 == orientation)
		{
			if (!IsEmpty(in splitRect))
			{
				int nextSibling = reference.NextSibling;
				num2 = NewNode();
				m_Nodes[num2] = new Node
				{
					Parent = reference.Parent,
					NextSibling = nextSibling,
					PrevSibling = num,
					Rect = splitRect,
					Kind = NodeKind.Free,
					Orientation = orientation
				};
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num).NextSibling = num2;
				if (nextSibling != int.MaxValue)
				{
					UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, nextSibling).PrevSibling = num2;
				}
			}
			else
			{
				num2 = int.MaxValue;
			}
			if (!IsEmpty(in leftoverRect))
			{
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num).Kind = NodeKind.Container;
				num3 = NewNode();
				num4 = NewNode();
				m_Nodes[num3] = new Node
				{
					Parent = num,
					NextSibling = num4,
					PrevSibling = int.MaxValue,
					Rect = rect,
					Kind = NodeKind.Alloc,
					Orientation = Flip(orientation)
				};
				m_Nodes[num4] = new Node
				{
					Parent = num,
					NextSibling = int.MaxValue,
					PrevSibling = num3,
					Rect = leftoverRect,
					Kind = NodeKind.Free,
					Orientation = Flip(orientation)
				};
			}
			else
			{
				num3 = num;
				ref Node reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num);
				reference2.Kind = NodeKind.Alloc;
				reference2.Rect = rect;
				num4 = int.MaxValue;
			}
		}
		else
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num).Kind = NodeKind.Container;
			if (!IsEmpty(in splitRect))
			{
				num2 = NewNode();
				m_Nodes[num2] = new Node
				{
					Parent = num,
					NextSibling = int.MaxValue,
					PrevSibling = int.MaxValue,
					Rect = splitRect,
					Kind = NodeKind.Free,
					Orientation = Flip(orientation)
				};
			}
			else
			{
				num2 = int.MaxValue;
			}
			if (!IsEmpty(in leftoverRect))
			{
				int num5 = NewNode();
				m_Nodes[num5] = new Node
				{
					Parent = num,
					NextSibling = num2,
					PrevSibling = int.MaxValue,
					Rect = default(NativeRectInt),
					Kind = NodeKind.Container,
					Orientation = Flip(orientation)
				};
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num2).PrevSibling = num5;
				num3 = NewNode();
				num4 = NewNode();
				m_Nodes[num3] = new Node
				{
					Parent = num5,
					NextSibling = num4,
					PrevSibling = int.MaxValue,
					Rect = rect,
					Kind = NodeKind.Alloc,
					Orientation = orientation
				};
				m_Nodes[num4] = new Node
				{
					Parent = num5,
					NextSibling = int.MaxValue,
					PrevSibling = num3,
					Rect = leftoverRect,
					Kind = NodeKind.Free,
					Orientation = orientation
				};
			}
			else
			{
				num3 = NewNode();
				m_Nodes[num3] = new Node
				{
					Parent = num,
					NextSibling = num2,
					PrevSibling = int.MaxValue,
					Rect = rect,
					Kind = NodeKind.Alloc,
					Orientation = Flip(orientation)
				};
				UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num2).PrevSibling = num3;
				num4 = int.MaxValue;
			}
		}
		if (num2 != int.MaxValue)
		{
			AddFreeRect(num2, splitRect.size);
		}
		if (num4 != int.MaxValue)
		{
			AddFreeRect(num4, leftoverRect.size);
		}
		Allocation result = default(Allocation);
		result.Id = AllocId(num3);
		result.NodeIndex = num3;
		result.Rect = rect;
		return result;
	}

	public void Deallocate(uint allocId)
	{
		int num = GetIndex(allocId);
		Span<Node> span = m_Nodes.AsArray().AsSpan();
		ref Node reference = ref span[num];
		reference.Kind = NodeKind.Free;
		while (true)
		{
			Orientation orientation = reference.Orientation;
			int nextSibling = reference.NextSibling;
			int prevSibling = reference.PrevSibling;
			if (nextSibling != int.MaxValue && span[nextSibling].Kind == NodeKind.Free)
			{
				MergeSiblings(num, nextSibling, orientation);
			}
			if (prevSibling != int.MaxValue && span[prevSibling].Kind == NodeKind.Free)
			{
				MergeSiblings(prevSibling, num, orientation);
				num = prevSibling;
				reference = ref span[num];
			}
			int parent = reference.Parent;
			if (parent == int.MaxValue || reference.PrevSibling != int.MaxValue || reference.NextSibling != int.MaxValue)
			{
				break;
			}
			ref Node reference2 = ref span[parent];
			MarkNodeUnused(num);
			reference2.Rect = reference.Rect;
			reference2.Kind = NodeKind.Free;
			num = parent;
			reference = ref span[num];
		}
		AddFreeRect(num, reference.Rect.size);
	}

	public void Grow(int2 newSize)
	{
		int2 size = m_Size;
		m_Size = newSize;
		int num = newSize.x - size.x;
		int num2 = newSize.y - size.y;
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, m_RootNode);
		if (reference.Kind == NodeKind.Free && math.all(reference.Rect.size == size))
		{
			reference.Rect.max = reference.Rect.min + newSize;
			return;
		}
		Orientation orientation = reference.Orientation;
		if (orientation switch
		{
			Orientation.Horizontal => num > 0, 
			Orientation.Vertical => num2 > 0, 
			_ => false, 
		})
		{
			int num3 = m_RootNode;
			ref Node reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num3);
			while (reference2.NextSibling != int.MaxValue)
			{
				num3 = reference2.NextSibling;
				reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num3);
			}
			ref Node reference3 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num3);
			if (reference3.Kind == NodeKind.Free)
			{
				switch (orientation)
				{
				case Orientation.Horizontal:
					reference3.Rect.max += new int2(num, 0);
					break;
				case Orientation.Vertical:
					reference3.Rect.max += new int2(0, num2);
					break;
				}
			}
			else
			{
				NativeRectInt rect = default(NativeRectInt);
				switch (orientation)
				{
				case Orientation.Horizontal:
				{
					int2 int2 = new int2(reference3.Rect.xMax, reference3.Rect.yMin);
					int2 max2 = int2 + new int2(num, reference3.Rect.height);
					NativeRectInt nativeRectInt = default(NativeRectInt);
					nativeRectInt.min = int2;
					nativeRectInt.max = max2;
					rect = nativeRectInt;
					break;
				}
				case Orientation.Vertical:
				{
					int2 @int = new int2(reference3.Rect.xMin, reference3.Rect.yMax);
					int2 max = @int + new int2(reference3.Rect.width, num2);
					NativeRectInt nativeRectInt = default(NativeRectInt);
					nativeRectInt.min = @int;
					nativeRectInt.max = max;
					rect = nativeRectInt;
					break;
				}
				}
				int num4 = (reference3.NextSibling = NewNode());
				m_Nodes[num4] = new Node
				{
					Kind = NodeKind.Free,
					Rect = rect,
					PrevSibling = num3,
					NextSibling = int.MaxValue,
					Parent = int.MaxValue,
					Orientation = orientation
				};
				AddFreeRect(num4, rect.size);
			}
		}
		if (orientation switch
		{
			Orientation.Horizontal => num2 > 0, 
			Orientation.Vertical => num > 0, 
			_ => false, 
		})
		{
			int num5 = NewNode();
			int num6 = NewNode();
			int rootNode = m_RootNode;
			m_RootNode = num6;
			Orientation orientation2 = Flip(orientation);
			int2 min = 0;
			switch (orientation2)
			{
			case Orientation.Horizontal:
				min = new int2(size.x, 0);
				break;
			case Orientation.Vertical:
				min = new int2(0, size.y);
				break;
			}
			int2 max3 = newSize;
			NativeRectInt nativeRectInt = default(NativeRectInt);
			nativeRectInt.min = min;
			nativeRectInt.max = max3;
			NativeRectInt rect2 = nativeRectInt;
			m_Nodes[num5] = new Node
			{
				Parent = int.MaxValue,
				PrevSibling = num6,
				NextSibling = int.MaxValue,
				Kind = NodeKind.Free,
				Rect = rect2,
				Orientation = orientation2
			};
			m_Nodes[num6] = new Node
			{
				Parent = int.MaxValue,
				PrevSibling = int.MaxValue,
				NextSibling = num5,
				Kind = NodeKind.Container,
				Rect = default(NativeRectInt),
				Orientation = orientation2
			};
			AddFreeRect(num5, rect2.size);
			int num7 = rootNode;
			while (num7 != int.MaxValue)
			{
				ref Node reference4 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num7);
				reference4.Parent = num6;
				num7 = reference4.NextSibling;
			}
			num7 = UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, rootNode).NextSibling;
			while (num7 != int.MaxValue)
			{
				ref Node reference5 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num7);
				reference5.Parent = num6;
				num7 = reference5.PrevSibling;
			}
		}
	}

	public void Clear()
	{
		m_Nodes.Clear();
		ref NativeList<Node> nodes = ref m_Nodes;
		Node value = new Node
		{
			Parent = int.MaxValue,
			NextSibling = int.MaxValue,
			PrevSibling = int.MaxValue,
			Rect = new NativeRectInt(0, m_Size),
			Kind = NodeKind.Free,
			Orientation = Orientation.Vertical
		};
		nodes.Add(in value);
		m_RootNode = 0;
		m_Generations.Clear();
		ref NativeList<int> generations = ref m_Generations;
		int value2 = 0;
		generations.Add(in value2);
		m_UnusedNodes = int.MaxValue;
		int bucketIndex;
		NativeList<int> freeListForSize = GetFreeListForSize(m_Size, out bucketIndex);
		for (int i = 0; i < 3; i++)
		{
			GetFreeList(i).Clear();
		}
		value2 = 0;
		freeListForSize.Add(in value2);
	}

	public void Reset(in int2 size, in AllocatorOptions options)
	{
		m_Alignment = options.Alignment;
		m_SmallSizeThreshold = options.SmallSizeThreshold;
		m_LargeSizeThreshold = options.LargeSizeThreshold;
		m_Size = size;
		Clear();
	}

	private void MergeSiblings(int nodeId, int nextId, Orientation orientation)
	{
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, nodeId);
		ref Node reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, nextId);
		NativeRectInt rect = reference2.Rect;
		int2 size = rect.size;
		switch (orientation)
		{
		case Orientation.Horizontal:
			reference.Rect.xMax += size.x;
			break;
		case Orientation.Vertical:
			reference.Rect.yMax += size.y;
			break;
		}
		int num = (reference.NextSibling = reference2.NextSibling);
		if (num != int.MaxValue)
		{
			UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num).PrevSibling = nodeId;
		}
		MarkNodeUnused(nextId);
	}

	private void MarkNodeUnused(int nodeId)
	{
		ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, nodeId);
		reference.Kind = NodeKind.Unused;
		reference.NextSibling = m_UnusedNodes;
		m_UnusedNodes = nodeId;
	}

	public int GetIndex(uint allocId)
	{
		int num = (int)(allocId & 0xFFFFFF);
		_ = m_Generations[num];
		return num;
	}

	private uint AllocId(int index)
	{
		uint num = (uint)m_Generations[index];
		return (uint)index + (num << 24);
	}

	private void AddFreeRect(int id, int2 size)
	{
		_ = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, id);
		GetFreeListForSize(size, out var _).Add(in id);
	}

	private Orientation Flip(Orientation orientation)
	{
		if (orientation == Orientation.Vertical)
		{
			return Orientation.Horizontal;
		}
		return Orientation.Vertical;
	}

	private int NewNode()
	{
		int unusedNodes = m_UnusedNodes;
		if (unusedNodes < m_Nodes.Length)
		{
			m_UnusedNodes = UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, unusedNodes).NextSibling;
			m_Generations[unusedNodes]++;
			return unusedNodes;
		}
		ref NativeList<Node> nodes = ref m_Nodes;
		Node value = new Node
		{
			Parent = int.MaxValue,
			NextSibling = int.MaxValue,
			PrevSibling = int.MaxValue,
			Rect = default(NativeRectInt),
			Kind = NodeKind.Unused,
			Orientation = Orientation.Horizontal
		};
		nodes.Add(in value);
		ref NativeList<int> generations = ref m_Generations;
		int value2 = 0;
		generations.Add(in value2);
		return m_Nodes.Length - 1;
	}

	private bool IsEmpty(in NativeRectInt rect)
	{
		int2 size = rect.size;
		if (size.x == 0 || size.y == 0)
		{
			return true;
		}
		return false;
	}

	private void GuillotineRect(in NativeRectInt chosenRect, int2 requestedSize, Orientation defaultOrientation, out NativeRectInt splitRect, out NativeRectInt leftoverRect, out Orientation orientation)
	{
		NativeRectInt nativeRectInt = default(NativeRectInt);
		nativeRectInt.min = chosenRect.min + new int2(requestedSize.x, 0);
		nativeRectInt.max = new int2(chosenRect.max.x, chosenRect.min.y + requestedSize.y);
		NativeRectInt nativeRectInt2 = nativeRectInt;
		nativeRectInt = default(NativeRectInt);
		nativeRectInt.min = chosenRect.min + new int2(0, requestedSize.y);
		nativeRectInt.max = new int2(chosenRect.min.x + requestedSize.x, chosenRect.max.y);
		NativeRectInt nativeRectInt3 = nativeRectInt;
		if (math.all(requestedSize == chosenRect.size))
		{
			orientation = defaultOrientation;
			splitRect = default(NativeRectInt);
			leftoverRect = default(NativeRectInt);
		}
		else if (nativeRectInt2.area > nativeRectInt3.area)
		{
			leftoverRect = nativeRectInt3;
			splitRect = new NativeRectInt
			{
				min = nativeRectInt2.min,
				max = new int2(nativeRectInt2.max.x, chosenRect.max.y)
			};
			orientation = Orientation.Horizontal;
		}
		else
		{
			leftoverRect = nativeRectInt2;
			splitRect = new NativeRectInt
			{
				min = nativeRectInt3.min,
				max = new int2(chosenRect.max.x, nativeRectInt3.max.y)
			};
			orientation = Orientation.Vertical;
		}
	}

	private void AdjustSize(int alignment, ref int size)
	{
		int num = size % alignment;
		if (num > 0)
		{
			size += alignment - num;
		}
	}

	private int FindSuitableRect(int2 requestedSize)
	{
		GetFreeListForSize(requestedSize, out var bucketIndex);
		bool flag = bucketIndex == 2;
		for (int i = 0; i < 3; i++)
		{
			int num = ((!flag) ? int.MaxValue : 0);
			(int, int)? tuple = null;
			int num2 = 0;
			while (num2 < GetFreeList(i).Length)
			{
				int num3 = GetFreeList(i)[num2];
				ref Node reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Nodes, num3);
				if (reference.Kind != NodeKind.Free)
				{
					GetFreeList(i).RemoveAtSwapBack(num2);
					continue;
				}
				int2 size = reference.Rect.size;
				int num4 = size.x - requestedSize.x;
				int num5 = size.y - requestedSize.y;
				if (num4 >= 0 && num5 >= 0)
				{
					if (num4 == 0 || num5 == 0)
					{
						tuple = (num3, num2);
						break;
					}
					int num6 = math.min(num4, num5);
					if ((flag && num6 > num) || (!flag && num6 < num))
					{
						num = num6;
						tuple = (num3, num2);
					}
				}
				num2++;
			}
			if (tuple.HasValue)
			{
				GetFreeList(i).RemoveAtSwapBack(tuple.Value.Item2);
				return tuple.Value.Item1;
			}
		}
		return int.MaxValue;
	}

	private NativeList<int> GetFreeListForSize(int2 size, out int bucketIndex)
	{
		if (size.x >= m_LargeSizeThreshold || size.y >= m_LargeSizeThreshold)
		{
			bucketIndex = 2;
			return m_LargeBucket;
		}
		if (size.x >= m_SmallSizeThreshold || size.y >= m_SmallSizeThreshold)
		{
			bucketIndex = 1;
			return m_MediumBucket;
		}
		bucketIndex = 0;
		return m_SmallBucket;
	}

	private NativeList<int> GetFreeList(int listIndex)
	{
		return listIndex switch
		{
			2 => m_LargeBucket, 
			1 => m_MediumBucket, 
			0 => m_SmallBucket, 
			_ => throw new ArgumentOutOfRangeException("listIndex"), 
		};
	}

	public float Occupancy()
	{
		Span<Node> span = m_Nodes.AsArray().AsSpan();
		float num = 0f;
		for (int i = 0; i < span.Length; i++)
		{
			ref Node reference = ref span[i];
			if (reference.Kind == NodeKind.Alloc)
			{
				num += (float)reference.Rect.area;
			}
		}
		return num / (float)(Width * Height);
	}
}
