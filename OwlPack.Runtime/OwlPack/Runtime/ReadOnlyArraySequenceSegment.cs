using System;

namespace OwlPack.Runtime;

public class ReadOnlyArraySequenceSegment
{
	protected readonly byte[] m_Array;

	public ReadOnlyArraySpan ArraySpan => new ReadOnlyArraySpan(m_Array, RunningIndex);

	public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(m_Array, 0, RunningIndex);

	public ReadOnlyArraySequenceSegment Next { get; protected set; }

	public int RunningIndex { get; protected set; }

	public ReadOnlyArraySequenceSegment(byte[] array, int initialIndex = 0)
	{
		if (initialIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("initialIndex");
		}
		m_Array = array;
		RunningIndex = initialIndex;
	}
}
