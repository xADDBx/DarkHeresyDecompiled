using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Experimental.Collections;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Experimental.Geometry;

[BurstCompile]
public struct UnsafeBvh<T> : IDisposable where T : unmanaged
{
	public struct Node
	{
		public T Data;

		public Aabb Box;

		public uint Parent;

		public unsafe fixed uint Children[2];

		public bool IsLeaf;
	}

	private enum Rotation
	{
		None,
		BF,
		BG,
		CD,
		CE
	}

	public const uint kInvalidIndex = uint.MaxValue;

	public UnsafePool<Node> Nodes;

	public uint NodesCount;

	public uint RootIndex;

	public bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Nodes.IsCreated;
		}
	}

	public UnsafeBvh(uint initialCapacity, Allocator allocator)
	{
		NodesCount = 0u;
		Nodes = new UnsafePool<Node>(initialCapacity, allocator);
		RootIndex = uint.MaxValue;
	}

	public void Dispose()
	{
		Nodes.Dispose();
		NodesCount = 0u;
		RootIndex = uint.MaxValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		Nodes.Clear();
		NodesCount = 0u;
		RootIndex = uint.MaxValue;
	}

	public unsafe void Delete(uint node)
	{
		uint parent = Nodes[node].Parent;
		if (parent != uint.MaxValue)
		{
			uint num = ((node == Nodes[parent].Children[0]) ? Nodes[parent].Children[1] : Nodes[parent].Children[0]);
			uint parent2 = Nodes[parent].Parent;
			Nodes[num].Parent = parent2;
			if (parent2 != uint.MaxValue)
			{
				if (parent == Nodes[parent2].Children[0])
				{
					Nodes[parent2].Children[0] = num;
				}
				else
				{
					Nodes[parent2].Children[1] = num;
				}
				Refit(parent2);
			}
			else
			{
				RootIndex = num;
			}
			Nodes.Deallocate(parent);
			NodesCount--;
		}
		else
		{
			RootIndex = uint.MaxValue;
		}
		Nodes.Deallocate(node);
		NodesCount--;
		AssertInvariant();
	}

	public unsafe uint Insert(T data, Aabb box)
	{
		uint num = AllocateLeafNode(data, box);
		if (RootIndex == uint.MaxValue)
		{
			RootIndex = num;
			NodesCount++;
			return num;
		}
		uint num2 = ChooseSiblingForLeafInsertion(in box);
		NodesCount++;
		uint parent = Nodes[num2].Parent;
		uint num3 = AllocateInternalNode();
		NodesCount++;
		Nodes[num3].Parent = parent;
		Nodes[num3].Box = Aabb.Union(in box, in Nodes[num2].Box);
		if (parent != uint.MaxValue)
		{
			if (Nodes[parent].Children[0] == num2)
			{
				Nodes[parent].Children[0] = num3;
			}
			else
			{
				Nodes[parent].Children[1] = num3;
			}
		}
		else
		{
			RootIndex = num3;
		}
		Nodes[num3].Children[0] = num2;
		Nodes[num3].Children[1] = num;
		Nodes[num2].Parent = num3;
		Nodes[num].Parent = num3;
		Refit(Nodes[num].Parent);
		AssertInvariant();
		return num;
	}

	private unsafe void Refit(uint idx)
	{
		while (idx != uint.MaxValue)
		{
			uint fixedElementField = Nodes[idx].Children[0];
			uint index = Nodes[idx].Children[1];
			Nodes[idx].Box = Aabb.Union(in Nodes[fixedElementField].Box, in Nodes[index].Box);
			Rotate(idx);
			idx = Nodes[idx].Parent;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void Rotate(uint idxA)
	{
		ref Node reference = ref Nodes[idxA];
		if (reference.IsLeaf)
		{
			Debug.Log("GOTCHA!");
			return;
		}
		uint fixedElementField = reference.Children[0];
		uint num = reference.Children[1];
		ref Node reference2 = ref Nodes[fixedElementField];
		ref Node reference3 = ref Nodes[num];
		Rotation rotation = Rotation.None;
		float num2 = 0f;
		Aabb box = default(Aabb);
		if (!reference3.IsLeaf)
		{
			uint fixedElementField2 = reference3.Children[0];
			uint index = reference3.Children[1];
			ref Node reference4 = ref Nodes[fixedElementField2];
			ref Node reference5 = ref Nodes[index];
			float area = reference3.Box.Area;
			Aabb aabb = Aabb.Union(in reference2.Box, in reference5.Box);
			Aabb aabb2 = Aabb.Union(in reference2.Box, in reference4.Box);
			float num3 = area - aabb.Area;
			float num4 = area - aabb2.Area;
			if (num3 > num4)
			{
				if (num3 > num2)
				{
					rotation = Rotation.BF;
					num2 = num3;
					box = aabb;
				}
			}
			else if (num4 > num2)
			{
				rotation = Rotation.BG;
				num2 = num4;
				box = aabb2;
			}
		}
		if (!reference2.IsLeaf)
		{
			uint fixedElementField3 = Nodes[fixedElementField].Children[0];
			uint index2 = Nodes[fixedElementField].Children[1];
			ref Node reference6 = ref Nodes[fixedElementField3];
			ref Node reference7 = ref Nodes[index2];
			float area2 = reference2.Box.Area;
			Aabb aabb3 = Aabb.Union(in reference3.Box, in reference7.Box);
			Aabb aabb4 = Aabb.Union(in reference3.Box, in reference6.Box);
			float num5 = area2 - aabb3.Area;
			float num6 = area2 - aabb4.Area;
			if (num5 > num6)
			{
				if (num5 > num2)
				{
					rotation = Rotation.CD;
					num2 = num5;
					box = aabb3;
				}
			}
			else if (num6 > num2)
			{
				rotation = Rotation.CE;
				num2 = num6;
				box = aabb4;
			}
		}
		switch (rotation)
		{
		case Rotation.BF:
		{
			uint fixedElementField5 = reference3.Children[0];
			ref Node reference11 = ref Nodes[fixedElementField5];
			reference2.Parent = num;
			reference3.Children[0] = fixedElementField;
			reference11.Parent = idxA;
			reference.Children[0] = fixedElementField5;
			Nodes[num].Box = box;
			break;
		}
		case Rotation.BG:
		{
			uint num8 = reference3.Children[1];
			ref Node reference10 = ref Nodes[num8];
			reference2.Parent = num;
			reference3.Children[1] = fixedElementField;
			reference10.Parent = idxA;
			reference.Children[0] = num8;
			reference3.Box = box;
			break;
		}
		case Rotation.CD:
		{
			uint fixedElementField4 = Nodes[fixedElementField].Children[0];
			ref Node reference9 = ref Nodes[fixedElementField4];
			reference3.Parent = fixedElementField;
			reference2.Children[0] = num;
			reference9.Parent = idxA;
			reference.Children[1] = fixedElementField4;
			reference2.Box = box;
			break;
		}
		case Rotation.CE:
		{
			uint num7 = Nodes[fixedElementField].Children[1];
			ref Node reference8 = ref Nodes[num7];
			reference3.Parent = fixedElementField;
			reference2.Children[1] = num;
			reference8.Parent = idxA;
			reference.Children[1] = num7;
			reference2.Box = box;
			break;
		}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe uint AllocateLeafNode(T data, Aabb box)
	{
		uint num = Nodes.Allocate();
		Nodes[num] = new Node
		{
			Data = data,
			Box = box,
			Parent = uint.MaxValue,
			IsLeaf = true
		};
		Nodes[num].Children[0] = uint.MaxValue;
		Nodes[num].Children[1] = uint.MaxValue;
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe uint AllocateInternalNode()
	{
		uint num = Nodes.Allocate();
		Nodes[num] = new Node
		{
			Data = default(T),
			Box = default(Aabb),
			Parent = uint.MaxValue,
			IsLeaf = false
		};
		Nodes[num].Children[0] = uint.MaxValue;
		Nodes[num].Children[1] = uint.MaxValue;
		return num;
	}

	private unsafe uint ChooseSiblingForLeafInsertion(in Aabb box)
	{
		if (NodesCount == 1)
		{
			return RootIndex;
		}
		float area = box.Area;
		ref Node reference = ref Nodes[RootIndex];
		float area2 = Aabb.Union(in reference.Box, in box).Area;
		float num = area2 - reference.Box.Area;
		float num2 = area + num;
		if (num2 >= area2)
		{
			return RootIndex;
		}
		uint result = RootIndex;
		float num3 = area2;
		UnsafePriorityQueue<(uint, float)> unsafePriorityQueue = new UnsafePriorityQueue<(uint, float)>(NodesCount, Allocator.Temp);
		try
		{
			(uint, float) value = (reference.Children[0], num);
			unsafePriorityQueue.Push(in value, 0f - num2);
			value = (reference.Children[1], num);
			unsafePriorityQueue.Push(in value, 0f - num2);
			do
			{
				var (num4, num5) = unsafePriorityQueue.Top();
				unsafePriorityQueue.Pop();
				float area3 = Aabb.Union(in Nodes[num4].Box, in box).Area;
				float num6 = area3 + num5;
				if (num6 < num3)
				{
					result = num4;
					num3 = num6;
				}
				if (!Nodes[num4].IsLeaf)
				{
					float num7 = num5 + (area3 - Nodes[num4].Box.Area);
					float num8 = area + num7;
					if (num8 < num3)
					{
						value = (Nodes[num4].Children[0], num7);
						unsafePriorityQueue.Push(in value, 0f - num8);
						value = (Nodes[num4].Children[0], num7);
						unsafePriorityQueue.Push(in value, 0f - num8);
					}
				}
			}
			while (unsafePriorityQueue.Length != 0);
			return result;
		}
		finally
		{
			((IDisposable)unsafePriorityQueue).Dispose();
		}
	}

	private void AssertInvariant()
	{
		if (RootIndex != uint.MaxValue)
		{
			AssertBounds(RootIndex);
		}
	}

	private unsafe void AssertBounds(uint idx)
	{
		Node node = Nodes[idx];
		if (!node.IsLeaf)
		{
			AssertBounds(node.Children[0]);
			AssertBounds(node.Children[1]);
		}
	}
}
