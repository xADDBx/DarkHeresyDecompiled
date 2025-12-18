namespace Owlcat.Runtime.Visual.Utilities;

internal struct InvasiveLinkedList<T> where T : class, IInvasiveLinkedListNode<T>
{
	public struct Enumerator
	{
		private T m_Next;

		private T m_Current;

		public T Current => m_Current;

		public Enumerator(T first)
		{
			m_Next = first;
			m_Current = null;
		}

		public bool MoveNext()
		{
			if (m_Next != null)
			{
				m_Current = m_Next;
				m_Next = m_Next.Next;
				return true;
			}
			return false;
		}
	}

	private T m_First;

	public void AddFirst(T item)
	{
		if (m_First != null)
		{
			m_First.Prev = item;
			item.Next = m_First;
		}
		m_First = item;
	}

	public void Remove(T item)
	{
		if (item.Next != null)
		{
			item.Next.Prev = item.Prev;
		}
		if (item.Prev != null)
		{
			item.Prev.Next = item.Next;
		}
		else
		{
			m_First = item.Next;
		}
		item.Prev = null;
		item.Next = null;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_First);
	}
}
