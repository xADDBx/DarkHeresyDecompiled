using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;

[Serializable]
public struct BitMask256 : IEquatable<BitMask256>, IComparable<BitMask256>, IEnumerable<int>, IEnumerable
{
	public struct Enumerator : IEnumerator<int>, IEnumerator, IDisposable
	{
		private readonly BitMask256 m_Mask;

		private BitMask256 m_RemainingMask;

		private int m_BitIndex;

		public int Current => m_BitIndex;

		object IEnumerator.Current => Current;

		public Enumerator(BitMask256 mask)
		{
			m_Mask = mask;
			m_RemainingMask = mask;
			m_BitIndex = -1;
		}

		public bool MoveNext()
		{
			do
			{
				m_BitIndex++;
				if ((long)m_BitIndex >= 256L || !m_RemainingMask.Any())
				{
					return false;
				}
			}
			while (!m_RemainingMask.GetBit(m_BitIndex));
			m_RemainingMask.SetBit(m_BitIndex, value: false);
			return true;
		}

		public void Reset()
		{
			m_BitIndex = -1;
			m_RemainingMask = m_Mask;
		}

		public void Dispose()
		{
		}
	}

	private const uint kBitsInSingleMask = 64u;

	private const uint kMaskCount = 4u;

	public const uint kTotalBits = 256u;

	[SerializeField]
	private ulong m_Mask0;

	[SerializeField]
	private ulong m_Mask1;

	[SerializeField]
	private ulong m_Mask2;

	[SerializeField]
	private ulong m_Mask3;

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void SetBit(int bitIndex, bool value)
	{
		ExtractMaskIndex(bitIndex, out var maskIndex, out var localBitIndex);
		ulong num = (ulong)(1L << localBitIndex);
		if (value)
		{
			Or(maskIndex, num);
		}
		else
		{
			And(maskIndex, ~num);
		}
	}

	public readonly BitMask256 And(in BitMask256 other)
	{
		BitMask256 result = default(BitMask256);
		result.m_Mask0 = m_Mask0 & other.m_Mask0;
		result.m_Mask1 = m_Mask1 & other.m_Mask1;
		result.m_Mask2 = m_Mask2 & other.m_Mask2;
		result.m_Mask3 = m_Mask3 & other.m_Mask3;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BitMask256 FirstBitsSet(int bitCount)
	{
		BitMask256 result = default(BitMask256);
		result.SetAllBitsUpTo(bitCount - 1);
		return result;
	}

	private void SetAllBitsUpTo(int bitIndex)
	{
		ExtractMaskIndex(bitIndex, out var maskIndex, out var localBitIndex);
		for (long num = 0L; num < maskIndex; num++)
		{
			Or(num, ulong.MaxValue);
		}
		if ((long)localBitIndex == 63)
		{
			Or(maskIndex, ulong.MaxValue);
			return;
		}
		ulong num2 = (ulong)(1L << localBitIndex + 1);
		Or(maskIndex, num2 - 1);
	}

	public readonly BitMask256 Or(in BitMask256 other)
	{
		BitMask256 result = default(BitMask256);
		result.m_Mask0 = m_Mask0 | other.m_Mask0;
		result.m_Mask1 = m_Mask1 | other.m_Mask1;
		result.m_Mask2 = m_Mask2 | other.m_Mask2;
		result.m_Mask3 = m_Mask3 | other.m_Mask3;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ExtractMaskIndex(int bitIndex, out long maskIndex, out int localBitIndex)
	{
		maskIndex = (long)bitIndex / 64L;
		localBitIndex = (int)((long)bitIndex % 64L);
	}

	private void And(long maskIndex, ulong mask)
	{
		if ((ulong)maskIndex <= 3uL)
		{
			switch (maskIndex)
			{
			case 0L:
				m_Mask0 &= mask;
				break;
			case 1L:
				m_Mask1 &= mask;
				break;
			case 2L:
				m_Mask2 &= mask;
				break;
			case 3L:
				m_Mask3 &= mask;
				break;
			}
		}
	}

	private void Or(long maskIndex, ulong mask)
	{
		if ((ulong)maskIndex <= 3uL)
		{
			switch (maskIndex)
			{
			case 0L:
				m_Mask0 |= mask;
				break;
			case 1L:
				m_Mask1 |= mask;
				break;
			case 2L:
				m_Mask2 |= mask;
				break;
			case 3L:
				m_Mask3 |= mask;
				break;
			}
		}
	}

	private readonly ulong GetMask(long maskIndex)
	{
		if ((ulong)maskIndex <= 3uL)
		{
			switch (maskIndex)
			{
			case 0L:
				return m_Mask0;
			case 1L:
				return m_Mask1;
			case 2L:
				return m_Mask2;
			case 3L:
				return m_Mask3;
			}
		}
		return m_Mask3;
	}

	public readonly bool GetBit(int bitIndex)
	{
		ExtractMaskIndex(bitIndex, out var maskIndex, out var localBitIndex);
		return (GetMask(maskIndex) & (ulong)(1L << localBitIndex)) != 0;
	}

	public readonly bool Any()
	{
		if (m_Mask0 == 0L && m_Mask1 == 0L && m_Mask2 == 0L)
		{
			return m_Mask3 != 0;
		}
		return true;
	}

	public bool Equals(BitMask256 other)
	{
		if (m_Mask0 == other.m_Mask0 && m_Mask1 == other.m_Mask1 && m_Mask2 == other.m_Mask2)
		{
			return m_Mask3 == other.m_Mask3;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BitMask256 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Mask0, m_Mask1, m_Mask2, m_Mask3);
	}

	public int CompareTo(BitMask256 other)
	{
		int num = m_Mask0.CompareTo(other.m_Mask0);
		if (num != 0)
		{
			return num;
		}
		int num2 = m_Mask1.CompareTo(other.m_Mask1);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = m_Mask2.CompareTo(other.m_Mask2);
		if (num3 != 0)
		{
			return num3;
		}
		return m_Mask3.CompareTo(other.m_Mask3);
	}
}
