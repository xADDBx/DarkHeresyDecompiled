using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Utility.DotNetExtensions;

public readonly struct FilteredList<T> : IEnumerable<T>, IEnumerable where T : class
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[CanBeNull]
		private readonly IList m_List;

		[CanBeNull]
		private readonly Func<T, bool> m_Filter;

		private int m_Index;

		public T Current
		{
			get
			{
				if (m_List != null && m_Index >= 0 && m_Index < m_List.Count)
				{
					return (T)m_List[m_Index];
				}
				return null;
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(IList list, Func<T, bool> filter)
		{
			m_List = list;
			m_Filter = filter;
			m_Index = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (m_List == null)
			{
				return false;
			}
			while (++m_Index < m_List.Count)
			{
				if (m_List[m_Index] is T arg)
				{
					Func<T, bool> filter = m_Filter;
					if (filter == null || filter(arg))
					{
						break;
					}
				}
			}
			return m_Index < m_List.Count;
		}

		void IEnumerator.Reset()
		{
			m_Index = -1;
		}
	}

	[CanBeNull]
	private readonly IList m_List;

	[CanBeNull]
	private readonly Func<T, bool> m_Filter;

	public FilteredList(IList list, Func<T, bool> filter = null)
	{
		m_List = list;
		m_Filter = filter;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_List, m_Filter);
	}

	public bool HasItem(T item)
	{
		if (m_List != null)
		{
			Func<T, bool> filter = m_Filter;
			if (filter == null || filter(item))
			{
				for (int i = 0; i < m_List.Count; i++)
				{
					if (m_List[i] == item)
					{
						return true;
					}
				}
				return false;
			}
		}
		return false;
	}

	public bool HasItem(Func<T, bool> pred)
	{
		if (m_List == null)
		{
			return false;
		}
		for (int i = 0; i < m_List.Count; i++)
		{
			if (m_List[i] is T arg)
			{
				Func<T, bool> filter = m_Filter;
				if ((filter == null || filter(arg)) && (pred == null || pred(arg)))
				{
					return true;
				}
			}
		}
		return false;
	}

	[CanBeNull]
	public T FirstItem(Func<T, bool> pred = null)
	{
		if (m_List == null)
		{
			return null;
		}
		for (int i = 0; i < m_List.Count; i++)
		{
			if (m_List[i] is T val)
			{
				Func<T, bool> filter = m_Filter;
				if ((filter == null || filter(val)) && (pred == null || pred(val)))
				{
					return val;
				}
			}
		}
		return null;
	}
}
