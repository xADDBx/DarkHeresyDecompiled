using System;
using System.Collections;
using System.Collections.Generic;

namespace OwlPack.Runtime;

public class ReadOnlyArraySequence : IEnumerable<ReadOnlyArraySequenceSegment>, IEnumerable
{
	private struct Enumerator : IEnumerator<ReadOnlyArraySequenceSegment>, IEnumerator, IDisposable
	{
		private ReadOnlyArraySequenceSegment m_First;

		private ReadOnlyArraySequenceSegment m_Current;

		public ReadOnlyArraySequenceSegment Current => m_Current;

		object IEnumerator.Current => m_Current;

		public Enumerator(ReadOnlyArraySequenceSegment segment)
		{
			m_First = segment;
			m_Current = null;
		}

		public void Dispose()
		{
			m_Current = null;
			m_First = null;
		}

		public bool MoveNext()
		{
			if (m_Current == null)
			{
				m_Current = m_First;
			}
			else
			{
				m_Current = m_Current.Next;
			}
			return m_Current != null;
		}

		public void Reset()
		{
			m_Current = m_First;
		}
	}

	private ReadOnlyArraySequenceSegment m_First;

	public int TotalLength { get; private set; }

	public ReadOnlyArraySequence(ReadOnlyArraySequenceSegment first, int totalLength)
	{
		m_First = first;
		TotalLength = totalLength;
	}

	public IEnumerator<ReadOnlyArraySequenceSegment> GetEnumerator()
	{
		return new Enumerator(m_First);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(m_First);
	}
}
