using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Allocators.MaxRectsBinPack.Native;

[BurstCompile]
public struct NativeMaxRectsBinPack : IDisposable
{
	public enum FreeRectChoiceHeuristic
	{
		RectBestShortSideFit,
		RectBestLongSideFit,
		RectBestAreaFit,
		RectBottomLeftRule,
		RectContactPointRule
	}

	[BurstCompile]
	private struct AllocationJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe NativeMaxRectsBinPack* Allocator;

		public NativeList<NativeRectInt> Rectangles;

		public unsafe void Execute()
		{
			Span<NativeRectInt> span = Rectangles.AsArray().AsSpan();
			int2 maxRectSize = 0;
			for (int i = 0; i < span.Length; i++)
			{
				ref NativeRectInt reference = ref span[i];
				maxRectSize.x = math.max(maxRectSize.x, reference.size.x);
				maxRectSize.y = math.max(maxRectSize.y, reference.size.y);
			}
			for (int j = 0; j < span.Length; j++)
			{
				ref NativeRectInt reference2 = ref span[j];
				NativeRectInt newNode = new NativeRectInt(reference2.position, reference2.size);
				while (!Allocator->Insert(reference2.width, reference2.height, FreeRectChoiceHeuristic.RectBestLongSideFit, out newNode))
				{
					Grow(in maxRectSize);
				}
				reference2.x = newNode.x;
				reference2.y = newNode.y;
			}
		}

		internal unsafe void Grow(in int2 maxRectSize)
		{
			int2 @int = maxRectSize;
			@int.x = math.max(@int.x, Allocator->Width);
			@int.y = math.max(@int.y, Allocator->Height);
			if (Allocator->Width < Allocator->Height)
			{
				Allocator->Grow(@int.x, 0);
			}
			else
			{
				Allocator->Grow(0, @int.y);
			}
			Allocator->PruneFreeList();
		}
	}

	private int m_BinWidth;

	private int m_BinHeight;

	private bool m_AllowRotations;

	private NativeList<NativeRectInt> m_UsedRectangles;

	private NativeList<NativeRectInt> m_FreeRectangles;

	public int Width => m_BinWidth;

	public int Height => m_BinHeight;

	public NativeList<NativeRectInt> UsedRectangles => m_UsedRectangles;

	public NativeList<NativeRectInt> FreeRectangles => m_FreeRectangles;

	public NativeMaxRectsBinPack(int width, int height, bool allowRotations)
	{
		m_BinWidth = width;
		m_BinHeight = height;
		m_AllowRotations = allowRotations;
		m_UsedRectangles = new NativeList<NativeRectInt>(Allocator.Persistent);
		m_FreeRectangles = new NativeList<NativeRectInt>(Allocator.Persistent);
		Init(width, height, allowRotations);
	}

	public void Init(int width, int height, bool allowRotations)
	{
		m_BinWidth = width;
		m_BinHeight = height;
		m_AllowRotations = allowRotations;
		NativeRectInt value = default(NativeRectInt);
		value.x = 0;
		value.y = 0;
		value.width = width;
		value.height = height;
		m_UsedRectangles.Clear();
		m_FreeRectangles.Clear();
		m_FreeRectangles.Add(in value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Insert(int width, int height, FreeRectChoiceHeuristic method, out NativeRectInt newNode)
	{
		newNode = default(NativeRectInt);
		int bestShortSideFit = 0;
		int bestLongSideFit = 0;
		switch (method)
		{
		case FreeRectChoiceHeuristic.RectBestShortSideFit:
			newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBottomLeftRule:
			newNode = FindPositionForNewNodeBottomLeft(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		case FreeRectChoiceHeuristic.RectContactPointRule:
			newNode = FindPositionForNewNodeContactPoint(width, height, ref bestShortSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBestLongSideFit:
			newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref bestLongSideFit, ref bestShortSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBestAreaFit:
			newNode = FindPositionForNewNodeBestAreaFit(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		}
		if (newNode.height == 0)
		{
			return false;
		}
		int num = m_FreeRectangles.Length;
		for (int i = 0; i < num; i++)
		{
			NativeRectInt freeNode = m_FreeRectangles[i];
			if (SplitFreeNode(in freeNode, ref newNode))
			{
				m_FreeRectangles.RemoveAt(i);
				i--;
				num--;
			}
		}
		PruneFreeList();
		m_UsedRectangles.Add(in newNode);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private NativeRectInt FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
	{
		NativeRectInt result = default(NativeRectInt);
		bestShortSideFit = int.MaxValue;
		Span<NativeRectInt> span = m_FreeRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			if (reference.width >= width && reference.height >= height)
			{
				int x = math.abs(reference.width - width);
				int y = math.abs(reference.height - height);
				int num = math.min(x, y);
				int num2 = math.max(x, y);
				if (num < bestShortSideFit || (num == bestShortSideFit && num2 < bestLongSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num;
					bestLongSideFit = num2;
				}
			}
			if (m_AllowRotations && reference.width >= height && reference.height >= width)
			{
				int a = Mathf.Abs(reference.width - height);
				int b = Mathf.Abs(reference.height - width);
				int num3 = Mathf.Min(a, b);
				int num4 = Mathf.Max(a, b);
				if (num3 < bestShortSideFit || (num3 == bestShortSideFit && num4 < bestLongSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestLongSideFit = num4;
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private NativeRectInt FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
	{
		NativeRectInt result = default(NativeRectInt);
		bestY = int.MaxValue;
		Span<NativeRectInt> span = m_FreeRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			if (reference.width >= width && reference.height >= height)
			{
				int num = reference.y + height;
				if (num < bestY || (num == bestY && reference.x < bestX))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = width;
					result.height = height;
					bestY = num;
					bestX = reference.x;
				}
			}
			if (m_AllowRotations && reference.width >= height && reference.height >= width)
			{
				int num2 = reference.y + width;
				if (num2 < bestY || (num2 == bestY && reference.x < bestX))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = height;
					result.height = width;
					bestY = num2;
					bestX = reference.x;
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private NativeRectInt FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
	{
		NativeRectInt result = default(NativeRectInt);
		bestLongSideFit = int.MaxValue;
		Span<NativeRectInt> span = m_FreeRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			if (reference.width >= width && reference.height >= height)
			{
				int a = Mathf.Abs(reference.width - width);
				int b = Mathf.Abs(reference.height - height);
				int num = Mathf.Min(a, b);
				int num2 = Mathf.Max(a, b);
				if (num2 < bestLongSideFit || (num2 == bestLongSideFit && num < bestShortSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num;
					bestLongSideFit = num2;
				}
			}
			if (m_AllowRotations && reference.width >= height && reference.height >= width)
			{
				int a2 = Mathf.Abs(reference.width - height);
				int b2 = Mathf.Abs(reference.height - width);
				int num3 = Mathf.Min(a2, b2);
				int num4 = Mathf.Max(a2, b2);
				if (num4 < bestLongSideFit || (num4 == bestLongSideFit && num3 < bestShortSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestLongSideFit = num4;
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private NativeRectInt FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
	{
		NativeRectInt result = default(NativeRectInt);
		bestAreaFit = int.MaxValue;
		Span<NativeRectInt> span = m_FreeRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			int num = reference.width * reference.height - width * height;
			if (reference.width >= width && reference.height >= height)
			{
				int a = Mathf.Abs(reference.width - width);
				int b = Mathf.Abs(reference.height - height);
				int num2 = Mathf.Min(a, b);
				if (num < bestAreaFit || (num == bestAreaFit && num2 < bestShortSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num2;
					bestAreaFit = num;
				}
			}
			if (m_AllowRotations && reference.width >= height && reference.height >= width)
			{
				int a2 = Mathf.Abs(reference.width - height);
				int b2 = Mathf.Abs(reference.height - width);
				int num3 = Mathf.Min(a2, b2);
				if (num < bestAreaFit || (num == bestAreaFit && num3 < bestShortSideFit))
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestAreaFit = num;
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private NativeRectInt FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
	{
		NativeRectInt result = default(NativeRectInt);
		bestContactScore = -1;
		Span<NativeRectInt> span = m_FreeRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			if (reference.width >= width && reference.height >= height)
			{
				int num = ContactPointScoreNode(reference.x, reference.y, width, height);
				if (num > bestContactScore)
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = width;
					result.height = height;
					bestContactScore = num;
				}
			}
			if (m_AllowRotations && reference.width >= height && reference.height >= width)
			{
				int num2 = ContactPointScoreNode(reference.x, reference.y, height, width);
				if (num2 > bestContactScore)
				{
					result.x = reference.x;
					result.y = reference.y;
					result.width = height;
					result.height = width;
					bestContactScore = num2;
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
	{
		if (i1end < i2start || i2end < i1start)
		{
			return 0;
		}
		return Mathf.Min(i1end, i2end) - Mathf.Max(i1start, i2start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int ContactPointScoreNode(int x, int y, int width, int height)
	{
		int num = 0;
		if (x == 0 || x + width == m_BinWidth)
		{
			num += height;
		}
		if (y == 0 || y + height == m_BinHeight)
		{
			num += width;
		}
		Span<NativeRectInt> span = m_UsedRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			if (reference.x == x + width || reference.x + reference.width == x)
			{
				num += CommonIntervalLength(reference.y, reference.y + reference.height, y, y + height);
			}
			if (reference.y == y + height || reference.y + reference.height == y)
			{
				num += CommonIntervalLength(reference.x, reference.x + reference.width, x, x + width);
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlaceRect(NativeRectInt node)
	{
		int num = m_FreeRectangles.Length;
		for (int i = 0; i < num; i++)
		{
			NativeRectInt freeNode = m_FreeRectangles[i];
			if (SplitFreeNode(in freeNode, ref node))
			{
				m_FreeRectangles.RemoveAt(i);
				i--;
				num--;
			}
		}
		PruneFreeList();
		m_UsedRectangles.Add(in node);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SplitFreeNode(in NativeRectInt freeNode, ref NativeRectInt usedNode)
	{
		if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
		{
			return false;
		}
		if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
		{
			if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
			{
				NativeRectInt value = freeNode;
				value.height = usedNode.y - value.y;
				m_FreeRectangles.Add(in value);
			}
			if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
			{
				NativeRectInt value2 = freeNode;
				value2.y = usedNode.y + usedNode.height;
				value2.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
				m_FreeRectangles.Add(in value2);
			}
		}
		if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
		{
			if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
			{
				NativeRectInt value3 = freeNode;
				value3.width = usedNode.x - value3.x;
				m_FreeRectangles.Add(in value3);
			}
			if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
			{
				NativeRectInt value4 = freeNode;
				value4.x = usedNode.x + usedNode.width;
				value4.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
				m_FreeRectangles.Add(in value4);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PruneFreeList()
	{
		for (int i = 0; i < m_FreeRectangles.Length; i++)
		{
			for (int j = i + 1; j < m_FreeRectangles.Length; j++)
			{
				NativeRectInt a = m_FreeRectangles[i];
				NativeRectInt b = m_FreeRectangles[j];
				if (IsContainedIn(in a, in b))
				{
					m_FreeRectangles.RemoveAt(i);
					i--;
					break;
				}
				a = m_FreeRectangles[j];
				b = m_FreeRectangles[i];
				if (IsContainedIn(in a, in b))
				{
					m_FreeRectangles.RemoveAt(j);
					j--;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsContainedIn(in NativeRectInt a, in NativeRectInt b)
	{
		if (a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width)
		{
			return a.y + a.height <= b.y + b.height;
		}
		return false;
	}

	public void Grow(int additionaWidth, int additionaHeight)
	{
		if (additionaWidth > 0)
		{
			ref NativeList<NativeRectInt> freeRectangles = ref m_FreeRectangles;
			NativeRectInt value = new NativeRectInt(m_BinWidth, 0, additionaWidth, m_BinHeight);
			freeRectangles.Add(in value);
			m_BinWidth += additionaWidth;
		}
		if (additionaHeight > 0)
		{
			ref NativeList<NativeRectInt> freeRectangles2 = ref m_FreeRectangles;
			NativeRectInt value = new NativeRectInt(0, m_BinHeight, m_BinWidth, additionaHeight);
			freeRectangles2.Add(in value);
			m_BinHeight += additionaHeight;
		}
	}

	public float Occupancy()
	{
		ulong num = 0uL;
		Span<NativeRectInt> span = m_UsedRectangles.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativeRectInt reference = ref span[i];
			num += (uint)(reference.width * reference.height);
		}
		return (float)num / (float)(m_BinWidth * m_BinHeight);
	}

	public void Dispose()
	{
		if (m_UsedRectangles.IsCreated)
		{
			m_UsedRectangles.Dispose();
		}
		if (m_FreeRectangles.IsCreated)
		{
			m_FreeRectangles.Dispose();
		}
	}
}
