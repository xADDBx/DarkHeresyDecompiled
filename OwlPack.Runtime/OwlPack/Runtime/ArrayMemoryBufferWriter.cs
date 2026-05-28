using System;
using System.Buffers;

namespace OwlPack.Runtime;

public class ArrayMemoryBufferWriter : IMemoryBufferWriter, IBufferWriter<byte>
{
	private ArrayBufferWriter<byte> m_Writer = new ArrayBufferWriter<byte>();

	public int Capacity => m_Writer.Capacity;

	public ReadOnlyArraySequence WrittenMemory => new ReadOnlyArraySequence(new ReadOnlyArraySequenceSegment(m_Writer.Buffer, m_Writer.Index), m_Writer.Index);

	public void Reset()
	{
		m_Writer.Clear();
	}

	public void Advance(int count)
	{
		m_Writer.Advance(count);
	}

	public Memory<byte> GetMemory(int sizeHint = 0)
	{
		return m_Writer.GetMemory(sizeHint);
	}

	public Span<byte> GetSpan(int sizeHint = 0)
	{
		return m_Writer.GetSpan(sizeHint);
	}
}
