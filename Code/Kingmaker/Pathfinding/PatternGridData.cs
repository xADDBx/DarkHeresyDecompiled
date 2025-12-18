using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct PatternGridData : IDisposable, IReadOnlyCollection<Vector2Int>, IEnumerable<Vector2Int>, IEnumerable
{
	public struct Enumerator : IEnumerator<Vector2Int>, IEnumerator, IDisposable
	{
		private readonly BitArray m_Offsets;

		private readonly IntRect m_Bounds;

		private int m_Current;

		public Vector2Int Current
		{
			get
			{
				if (m_Current < 0 || m_Current >= m_Offsets.Length)
				{
					throw new InvalidOperationException("Iterator is in invalid state");
				}
				return new Vector2Int(m_Bounds.xmin + m_Current % m_Bounds.Width, m_Bounds.ymin + m_Current / m_Bounds.Width);
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(BitArray offsets, IntRect bounds)
		{
			this = default(Enumerator);
			m_Offsets = offsets;
			m_Bounds = bounds;
			Reset();
		}

		public bool MoveNext()
		{
			if (m_Offsets == null)
			{
				return false;
			}
			do
			{
				m_Current++;
				if (m_Current >= m_Offsets.Length)
				{
					return false;
				}
			}
			while (!m_Offsets[m_Current]);
			return true;
		}

		public void Reset()
		{
			m_Current = -1;
		}

		public void Dispose()
		{
			Reset();
		}
	}

	public class ObjectPool<T>
	{
		private readonly ConcurrentBag<T> m_Objects;

		private readonly Func<T> m_ObjectGenerator;

		private readonly Action<T> m_Prepare;

		private readonly Action<T> m_Cleanup;

		public ObjectPool(Func<T> objectGenerator, Action<T> prepare, Action<T> cleanup)
		{
			m_ObjectGenerator = objectGenerator ?? throw new ArgumentNullException("objectGenerator");
			m_Prepare = prepare;
			m_Cleanup = cleanup;
			m_Objects = new ConcurrentBag<T>();
		}

		public T Get()
		{
			if (!m_Objects.TryTake(out var result))
			{
				result = m_ObjectGenerator();
			}
			m_Prepare(result);
			return result;
		}

		public void Return([CanBeNull] T item)
		{
			if (item != null)
			{
				m_Cleanup(item);
				m_Objects.Add(item);
			}
		}
	}

	public enum HaloMode
	{
		IncludeOriginalPattern,
		ExcludeOriginalPattern
	}

	public static PatternGridData Empty = new PatternGridData(new BitArray(0), new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue), 0, disposable: false);

	private static readonly IntRect Invalid = new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

	private static readonly ObjectPool<BitArray> Pool = new ObjectPool<BitArray>(() => new BitArray(0), delegate
	{
	}, delegate
	{
	});

	private readonly BitArray m_IncludedNodes;

	private readonly BitArray m_ExcludedNodes;

	private readonly IntRect m_Bounds;

	private readonly bool m_Disposable;

	private readonly int m_Count;

	public IntRect Bounds => m_Bounds;

	public bool IsEmpty
	{
		get
		{
			BitArray includedNodes = m_IncludedNodes;
			return includedNodes == null || includedNodes.Length <= 0;
		}
	}

	public int Count => m_Count;

	public bool Contains(Vector2Int item)
	{
		bool num = item.x >= m_Bounds.xmin && item.x <= m_Bounds.xmax;
		bool flag = item.y >= m_Bounds.ymin && item.y <= m_Bounds.ymax;
		if (!num || !flag)
		{
			return false;
		}
		int num2 = (item.y - m_Bounds.ymin) * m_Bounds.Width + (item.x - m_Bounds.xmin);
		if (num2 < 0 || num2 >= m_IncludedNodes.Length)
		{
			PFLog.Default.Error($"Coordinates are out of pattern: {item}");
			return false;
		}
		return m_IncludedNodes[num2];
	}

	public bool ContainsExcluded(Vector2Int item)
	{
		if (m_ExcludedNodes == null)
		{
			return false;
		}
		bool num = item.x >= m_Bounds.xmin && item.x <= m_Bounds.xmax;
		bool flag = item.y >= m_Bounds.ymin && item.y <= m_Bounds.ymax;
		if (!num || !flag)
		{
			return false;
		}
		int num2 = (item.y - m_Bounds.ymin) * m_Bounds.Width + (item.x - m_Bounds.xmin);
		if (num2 < 0 || num2 >= m_ExcludedNodes.Length)
		{
			PFLog.Default.Error($"Coordinates are out of pattern: {item}");
			return false;
		}
		return m_ExcludedNodes[num2];
	}

	public bool ContainsAny(Vector2Int item)
	{
		bool num = item.x >= m_Bounds.xmin && item.x <= m_Bounds.xmax;
		bool flag = item.y >= m_Bounds.ymin && item.y <= m_Bounds.ymax;
		if (!num || !flag)
		{
			return false;
		}
		int num2 = (item.y - m_Bounds.ymin) * m_Bounds.Width + (item.x - m_Bounds.xmin);
		if (num2 < 0 || num2 >= m_IncludedNodes.Length || (m_ExcludedNodes != null && num2 >= m_ExcludedNodes.Length))
		{
			PFLog.Default.Error($"Coordinates are out of pattern: {item}");
			return false;
		}
		if (!m_IncludedNodes[num2])
		{
			if (m_ExcludedNodes != null)
			{
				return m_ExcludedNodes[num2];
			}
			return false;
		}
		return true;
	}

	public PatternGridData Move(Vector2Int offset)
	{
		return new PatternGridData(m_IncludedNodes, m_ExcludedNodes, m_Bounds.Move(offset), m_Count, m_Disposable);
	}

	public static PatternGridData Create(HashSet<Vector2Int> pattern, bool disposable)
	{
		return Create(pattern, null, disposable);
	}

	public static PatternGridData Create(HashSet<Vector2Int> pattern, HashSet<Vector2Int> excludedFromPattern, bool disposable)
	{
		if (pattern.Count == 0 && (excludedFromPattern == null || excludedFromPattern.Count <= 0))
		{
			return Empty;
		}
		BitArray bitArray = Pool.Get();
		BitArray bitArray2 = Pool.Get();
		IntRect bounds = CalculateBounds(pattern, excludedFromPattern);
		ToBitArray(in pattern, in bounds, bitArray);
		ToBitArray(in excludedFromPattern, in bounds, bitArray2);
		return new PatternGridData(bitArray, bitArray2, bounds, pattern.Count, disposable);
	}

	public static PatternGridData Create(BitPattern2D pattern, bool disposable)
	{
		if (pattern.Count == 0)
		{
			return Empty;
		}
		BitArray result = Pool.Get();
		return new PatternGridData(pattern.GetCulledArray(in result), pattern.Bounds, pattern.Count, disposable);
	}

	public static PatternGridData Create(Linecast.Ray2NodeOffsets pattern, int length, bool disposable)
	{
		if (length == 0)
		{
			return Empty;
		}
		BitArray bitArray = Pool.Get();
		IntRect bounds = ToBitArray(in pattern, length, bitArray);
		return new PatternGridData(bitArray, bounds, length, disposable);
	}

	public PatternGridData BuildHalo(int haloSize, HaloMode haloMode, Vector2Int offset, Vector3 direction, bool preventBlowback, bool disposable)
	{
		PatternGridData patternGridData = this;
		IntRect bounds = patternGridData.Bounds.Expand(haloSize);
		int num = 0;
		BitArray bitArray = new BitArray(bounds.Width * bounds.Height);
		BitArray includedNodes = patternGridData.m_IncludedNodes;
		new Vector3(patternGridData.Bounds.xmin + haloSize, 0f, patternGridData.Bounds.ymin + haloSize);
		for (int i = 0; i < patternGridData.Bounds.Height; i++)
		{
			for (int j = 0; j < patternGridData.Bounds.Width; j++)
			{
				int index = i * patternGridData.Bounds.Width + j;
				if (!includedNodes[index])
				{
					continue;
				}
				for (int k = -haloSize; k <= haloSize; k++)
				{
					for (int l = -haloSize; l <= haloSize; l++)
					{
						int num2 = j + l;
						int num3 = i + k;
						if (haloMode == HaloMode.ExcludeOriginalPattern && num2 >= 0 && num2 < patternGridData.Bounds.Width && num3 >= 0 && num3 < patternGridData.Bounds.Height)
						{
							int index2 = num3 * patternGridData.Bounds.Width + num2;
							if (includedNodes[index2])
							{
								continue;
							}
						}
						int num4 = j + l + haloSize;
						int num5 = i + k + haloSize;
						if (num4 >= 0 && num4 < bounds.Width && num5 >= 0 && num5 < bounds.Height && (!preventBlowback || !(Vector3.Dot(new Vector3(num2 + (patternGridData.Bounds.xmin - offset.x), 0f, num3 + (patternGridData.Bounds.ymin - offset.y)), direction) < 0f)))
						{
							int index3 = num5 * bounds.Width + num4;
							bitArray[index3] = true;
							num++;
						}
					}
				}
			}
		}
		return new PatternGridData(bitArray, bounds, num, disposable);
	}

	private static IntRect CalculateBounds(HashSet<Vector2Int> nodes, HashSet<Vector2Int> excludedNodes)
	{
		if (nodes.Count == 0 && (excludedNodes == null || excludedNodes.Count <= 0))
		{
			return Invalid;
		}
		IntRect result = Invalid;
		foreach (Vector2Int node in nodes)
		{
			result = result.ExpandToContain(node.x, node.y);
		}
		if (excludedNodes != null)
		{
			foreach (Vector2Int excludedNode in excludedNodes)
			{
				result = result.ExpandToContain(excludedNode.x, excludedNode.y);
			}
		}
		return result;
	}

	private static void ToBitArray([CanBeNull] in HashSet<Vector2Int> nodes, in IntRect bounds, [NotNull] BitArray result)
	{
		if (bounds == Invalid)
		{
			result.Length = 0;
			return;
		}
		Vector2Int vector2Int = new Vector2Int(bounds.xmin, bounds.ymin);
		result.Length = bounds.Width * bounds.Height;
		result.SetAll(value: false);
		if (nodes == null)
		{
			return;
		}
		foreach (Vector2Int node in nodes)
		{
			Vector2Int vector2Int2 = node - vector2Int;
			int index = vector2Int2.y * bounds.Width + vector2Int2.x;
			result[index] = true;
		}
	}

	private static IntRect ToBitArray(in Linecast.Ray2NodeOffsets nodes, int length, BitArray result)
	{
		if (length == 0)
		{
			result.Length = 0;
			return Invalid;
		}
		IntRect result2 = Invalid;
		int num = 0;
		foreach (Vector2Int node in nodes)
		{
			if (++num > length)
			{
				break;
			}
			result2 = result2.ExpandToContain(node.x, node.y);
		}
		Vector2Int vector2Int = new Vector2Int(result2.xmin, result2.ymin);
		result.Length = result2.Width * result2.Height;
		result.SetAll(value: false);
		num = 0;
		foreach (Vector2Int node2 in nodes)
		{
			if (++num > length)
			{
				break;
			}
			Vector2Int vector2Int2 = node2 - vector2Int;
			int index = vector2Int2.y * result2.Width + vector2Int2.x;
			result[index] = true;
		}
		return result2;
	}

	private PatternGridData(BitArray includedNodes, IntRect bounds, int count, bool disposable)
		: this(includedNodes, null, bounds, count, disposable)
	{
	}

	private PatternGridData(BitArray includedNodes, [CanBeNull] BitArray excludedNodes, IntRect bounds, int count, bool disposable)
	{
		m_IncludedNodes = includedNodes;
		m_ExcludedNodes = excludedNodes;
		m_Bounds = bounds;
		m_Count = count;
		m_Disposable = disposable;
	}

	public void Dispose()
	{
		if (m_Disposable)
		{
			Pool.Return(m_IncludedNodes);
			Pool.Return(m_ExcludedNodes);
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_IncludedNodes, m_Bounds);
	}

	IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
